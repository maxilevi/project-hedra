import contextlib
import io
from pathlib import Path
import subprocess
import tempfile
import unittest

from checkout_assets import checkout, referenced_assets


def git(root, *arguments):
    return subprocess.check_output(['git', '-C', str(root), *arguments], text=True).strip()


class AssetCheckoutTests(unittest.TestCase):
    def test_manifest_rejects_paths_outside_asset_root(self):
        with tempfile.TemporaryDirectory() as directory:
            manifest = Path(directory) / 'project.csproj'
            manifest.write_text('<Project><None Include="../project-hedra-assets/../secret" /></Project>')
            with self.assertRaises(ValueError):
                referenced_assets(manifest)

    def test_sparse_checkout_uses_pinned_revision_and_ignores_authoring_files(self):
        with tempfile.TemporaryDirectory(prefix='asset checkout ') as directory:
            base = Path(directory)
            source = base / 'source'
            source.mkdir()
            git(source, 'init', '-q')
            git(source, 'config', 'user.name', 'CI test')
            git(source, 'config', 'user.email', 'ci@example.invalid')
            (source / 'runtime file.ply').write_text('pinned model')
            (source / 'unused.blend').write_text('authoring file')
            git(source, 'add', '.')
            git(source, 'commit', '-qm', 'Pinned assets')
            pinned = git(source, 'rev-parse', 'HEAD')
            (source / 'runtime file.ply').write_text('newer model')
            git(source, 'commit', '-qam', 'Newer assets')
            root = base / 'game'
            (root / 'Hedra').mkdir(parents=True)
            git(root, 'init', '-q')
            git(root, 'config', 'user.name', 'CI test')
            git(root, 'config', 'user.email', 'ci@example.invalid')
            (root / '.gitmodules').write_text(
                '[submodule "project-hedra-assets"]\npath = project-hedra-assets\n'
                f'url = {source.as_uri()}\n')
            (root / 'Hedra/Hedra.csproj').write_text(
                '<Project><None Include="..\\project-hedra-assets\\runtime file.ply" /></Project>')
            git(root, 'add', '.')
            git(root, 'update-index', '--add', '--cacheinfo', f'160000,{pinned},project-hedra-assets')
            git(root, 'commit', '-qm', 'Game manifest')
            with contextlib.redirect_stdout(io.StringIO()):
                checkout(root)
            assets = root / 'project-hedra-assets'
            self.assertEqual(git(assets, 'rev-parse', 'HEAD'), pinned)
            self.assertEqual((assets / 'runtime file.ply').read_text(), 'pinned model')
            self.assertFalse((assets / 'unused.blend').exists())
            with self.assertRaises(ValueError):
                checkout(root)


if __name__ == '__main__':
    unittest.main()
