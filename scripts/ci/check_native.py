#!/usr/bin/env python3
"""Exercise the native ABI without assets, a GPU, Steam, or the game loop."""
import argparse
import ctypes
import math
from pathlib import Path
import platform
import statistics
import time

NOISE_TYPES = (
    'Perlin', 'PerlinFractal', 'Simplex', 'SimplexFractal', 'Cubic',
    'CubicFractal', 'Cellular', 'Value', 'ValueFractal',
)


def default_library():
    system = platform.system()
    relative = {
        'Windows': 'windows-x64/Release/hedracore.dll',
        'Linux': 'linux-x64/libhedracore.so',
        'Darwin': 'macos-x64/libhedracore.dylib',
    }[system]
    return Path('.ci/native') / relative


class NativeNoise:
    def __init__(self, path):
        self.library = ctypes.CDLL(str(Path(path).resolve()))
        signatures = {
            'fastnoise_createObject': ([ctypes.c_int], ctypes.c_void_p),
            'fastnoise_deleteObject': ([ctypes.c_void_p], None),
            'fastnoise_freeNoise': ([ctypes.c_void_p], None),
            'fastnoise_setPerturbType': ([ctypes.c_void_p, ctypes.c_int], None),
        }
        for noise_type in NOISE_TYPES:
            signatures[f'fastnoise_get{noise_type}Set'] = (
                [ctypes.c_void_p] + [ctypes.c_float] * 3 + [ctypes.c_int] * 3
                + [ctypes.c_float] * 3, ctypes.POINTER(ctypes.c_float),
            )
        for name, (arguments, result) in signatures.items():
            function = getattr(self.library, name)
            function.argtypes = arguments
            function.restype = result

    def sample(self, noise_type, seed, perturbation, offset, shape):
        noise = self.library.fastnoise_createObject(seed)
        if not noise:
            raise RuntimeError('Native noise object allocation failed')
        try:
            self.library.fastnoise_setPerturbType(noise, perturbation)
            function = getattr(self.library, f'fastnoise_get{noise_type}Set')
            values = function(noise, *offset, *shape, 1.0, 1.0, 1.0)
            if not values:
                raise RuntimeError('Native sample allocation failed')
            try:
                result = tuple(values[index] for index in range(math.prod(shape)))
                # The legacy SIMD library can produce NaNs in normalization modes.
                # Preserve those bytes in regression comparisons; require finite
                # samples for the non-normalizing modes used by terrain generation.
                if perturbation < 3 and not all(math.isfinite(value) for value in result):
                    raise AssertionError(f'{noise_type} produced non-finite samples: seed={seed}, perturbation={perturbation}, offset={offset}, shape={shape}')
                return ctypes.string_at(values, math.prod(shape) * ctypes.sizeof(ctypes.c_float))
            finally:
                self.library.fastnoise_freeNoise(values)
        finally:
            self.library.fastnoise_deleteObject(noise)


def verify(current, baseline=None):
    cases = 0
    for noise_type in NOISE_TYPES:
        for seed in (1337, -123):
            for perturbation in range(6):
                for offset, shape in (
                    ((1.0, 2.0, 3.0), (4, 4, 4)),
                    ((-11.0, 7.0, -3.0), (7, 3, 5)),
                ):
                    args = noise_type, seed, perturbation, offset, shape
                    actual = current.sample(*args)
                    if actual != current.sample(*args):
                        raise AssertionError(f'Non-deterministic native noise: {args}')
                    if baseline and actual != baseline.sample(*args):
                        raise AssertionError(f'Native noise differs from baseline: {args}')
                    cases += 1
    print(f'Native ABI and determinism passed: {cases} cases')
    if baseline:
        print('All samples match the previous native revision bit for bit')


def benchmark(native):
    # An opt-in CPU microbenchmark; it does not measure world loading or gameplay.
    args = 'SimplexFractal', 1337, 0, (-32.0, 0.0, 16.0), (32, 32, 32)
    for _ in range(3):
        native.sample(*args)
    samples = []
    for _ in range(11):
        start = time.perf_counter()
        native.sample(*args)
        samples.append((time.perf_counter() - start) * 1000)
    print(f'NOISE_BENCH median_ms={statistics.median(samples):.3f} '
          f'samples=32768 iterations=11 platform={platform.platform()} '
          f'python={platform.python_version()} (includes ctypes copying and validation)')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--library', type=Path, default=default_library())
    parser.add_argument('--baseline', type=Path)
    parser.add_argument('--benchmark', action='store_true')
    options = parser.parse_args()
    native = NativeNoise(options.library)
    verify(native, NativeNoise(options.baseline) if options.baseline else None)
    if options.benchmark:
        benchmark(native)
