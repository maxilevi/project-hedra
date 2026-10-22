"""Failure-path checks for publishing and native builds; no network or SDK needed."""
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]


class BuildScriptTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix='hedra ci ')
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        (self.root / 'scripts').mkdir()
        self.bin = self.root / 'mock bin'
        self.bin.mkdir()
        self.environment = dict(os.environ, PATH=str(self.bin) + os.pathsep + os.environ['PATH'])
        for name in ('publish.sh', 'build-hedracore.sh'):
            shutil.copy2(ROOT / 'scripts' / name, self.root / 'scripts' / name)
        self.command('dotnet', '''#!/usr/bin/env python3
import os
from pathlib import Path
import sys
if os.environ.get('FAIL_PUBLISH'):
    sys.exit(37)
output = Path(sys.argv[sys.argv.index('--output') + 1])
output.mkdir(parents=True, exist_ok=True)
(output / 'Hedra').write_text('executable')
if not os.environ.get('NO_EXECUTABLE'):
    (output / 'Hedra').chmod(0o755)
for name in ('Assets', 'Shaders', 'Sounds', 'ref'):
    (output / name).mkdir()
config = sys.argv[sys.argv.index('-c') + 1]
assets = Path('.ci/assets') / config
assets.mkdir(parents=True, exist_ok=True)
for index in range(1, 4):
    if str(index) != os.environ.get('MISSING_ARCHIVE'):
        (assets / f'data{index}.db').write_text(f'archive {index}')
''')

    def command(self, name, body):
        command = self.bin / name
        command.write_text(body)
        command.chmod(0o755)

    def run_script(self, name, *arguments, **environment):
        return subprocess.run(
            ['bash', str(self.root / 'scripts' / name), *arguments],
            env=dict(self.environment, **environment), cwd='/',
            text=True, capture_output=True, check=False,
        )

    def test_failed_publish_propagates_exit_code(self):
        result = self.run_script('publish.sh', 'linux-x64', 'Release', FAIL_PUBLISH='1')
        self.assertEqual(result.returncode, 37, result.stderr)
        self.assertFalse((self.root / '.ci/publish/assets').exists())

    def test_invalid_configuration_and_rid_are_rejected(self):
        for arguments in ((), ('linux-x64',), ('unknown', 'Release'), ('win-x64', 'bad;exit 0')):
            with self.subTest(arguments=arguments):
                result = self.run_script('publish.sh', *arguments)
                self.assertEqual(result.returncode, 2, result.stderr)
                self.assertFalse((self.root / '.ci').exists())

    def test_debug_packaging_uses_its_own_directory(self):
        sentinel = self.root / 'Hedra/bin/Release/sentinel'
        sentinel.parent.mkdir(parents=True)
        sentinel.write_text('keep')
        result = self.run_script('publish.sh', 'linux-x64', 'Debug')
        self.assertEqual(result.returncode, 0, result.stderr)
        output = self.root / '.ci/publish/linux-x64'
        self.assertTrue((output / 'Hedra').is_file())
        for name in ('Assets', 'Shaders', 'Sounds', 'ref', 'data1.db', 'data2.db', 'data3.db'):
            self.assertFalse((output / name).exists())
        self.assertEqual(len(list((self.root / '.ci/publish/assets').glob('*.db'))), 3)
        self.assertEqual(sentinel.read_text(), 'keep')

    def test_missing_archive_fails(self):
        result = self.run_script('publish.sh', 'win-x64', 'Release', MISSING_ARCHIVE='2')
        self.assertNotEqual(result.returncode, 0)

    def test_shared_assets_are_refreshed_from_the_tested_build(self):
        assets = self.root / '.ci/publish/assets'
        assets.mkdir(parents=True)
        (assets / 'data1.db').write_text('different archive')
        result = self.run_script('publish.sh', 'win-x64', 'Release')
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual((assets / 'data1.db').read_text(), 'archive 1')

    def test_linux_requires_an_executable(self):
        result = self.run_script('publish.sh', 'linux-x64', 'Release', NO_EXECUTABLE='1')
        self.assertNotEqual(result.returncode, 0)

    def test_native_failure_is_not_hidden_by_copy(self):
        self.command('uname', '#!/bin/sh\necho Linux\n')
        self.command('cmake', '#!/bin/sh\nexit 19\n')
        result = self.run_script('build-hedracore.sh')
        self.assertEqual(result.returncode, 19, result.stderr)
        self.assertFalse((self.root / 'Hedra/x64/libhedracore.so').exists())

    def test_linux_builds_once_and_keeps_incremental_outputs(self):
        self.command('uname', '#!/bin/sh\necho Linux\n')
        self.command('cmake', '''#!/usr/bin/env python3
from pathlib import Path
import sys
arguments = sys.argv[1:]
if '-B' in arguments:
    build = Path(arguments[arguments.index('-B') + 1])
    build.mkdir(parents=True, exist_ok=True)
    with (build / 'configurations').open('a') as log:
        log.write('configured\\n')
else:
    build = Path(arguments[arguments.index('--build') + 1])
    (build / 'libhedracore.so').write_text('native library')
''')
        for _ in range(2):
            result = self.run_script('build-hedracore.sh')
            self.assertEqual(result.returncode, 0, result.stderr)
        native = self.root / '.ci/native'
        self.assertEqual([entry.name for entry in native.iterdir()], ['linux-x64'])
        self.assertEqual((native / 'linux-x64/configurations').read_text().count('configured'), 2)
        self.assertTrue((self.root / 'Hedra/x64/libhedracore.so').is_file())


if __name__ == '__main__':
    unittest.main()
