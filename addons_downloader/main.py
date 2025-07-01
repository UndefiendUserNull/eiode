import os
import argparse
import requests
import zipfile
import tarfile
import shutil
from concurrent.futures import ThreadPoolExecutor, as_completed
from rich.progress import (
    Progress,
    BarColumn,
    DownloadColumn,
    TextColumn,
    TimeRemainingColumn,
    TransferSpeedColumn
)

DEFAULT_URLS_FILE = "ADDONS.txt"
DEFAULT_DOWNLOAD_DIR = "addons"
DEFAULT_WORKERS = 4


def extract_file(filepath: str, extract_dir: str):
    """
    Extract a .zip or .tar.gz archive to the specified directory.
    """
    try:
        if filepath.endswith('.zip'):
            with zipfile.ZipFile(filepath, 'r') as zip_ref:
                zip_ref.extractall(extract_dir)
            print(f"[+] Extracted: {os.path.basename(filepath)}")
        elif filepath.endswith('.tar.gz') or filepath.endswith('.tgz'):
            with tarfile.open(filepath, 'r:gz') as tar_ref:
                tar_ref.extractall(extract_dir)
            print(f"[+] Extracted: {os.path.basename(filepath)}")
        else:
            print(f"[i] Skipping extraction (unsupported format): {os.path.basename(filepath)}")
    except Exception as e:
        print(f"[!] Failed to extract {os.path.basename(filepath)}: {e}")


def find_plugin_dirs(base_dir: str):
    """
    Recursively find directories containing either plugin.cfg or .gdextension.
    """
    plugin_dirs = []
    for root, dirs, files in os.walk(base_dir):
        if 'plugin.cfg' in files or any(f.endswith('.gdextension') for f in files):
            plugin_dirs.append(root)
    return plugin_dirs


def move_plugin_dirs(plugin_dirs, target_root):
    """
    Move plugin directories to the target root (addons).
    """
    moved_paths = []
    for src_dir in plugin_dirs:
        dst_dir = os.path.join(target_root, os.path.basename(src_dir))
        try:
            if os.path.abspath(src_dir) == os.path.abspath(dst_dir):
                moved_paths.append(dst_dir)
                continue

            if os.path.exists(dst_dir):
                shutil.rmtree(dst_dir)
            shutil.move(src_dir, dst_dir)
            moved_paths.append(dst_dir)
            print(f"[+] Moved: {src_dir} -> {dst_dir}")
        except Exception as e:
            print(f"[!] Failed to move {src_dir}: {e}")
    return moved_paths


def cleanup_non_plugins(base_dir: str, valid_plugin_paths):
    """
    Delete all files and folders in base_dir that are not valid plugins.
    """
    valid_plugin_paths = [os.path.abspath(p) for p in valid_plugin_paths]
    for item in os.listdir(base_dir):
        full_path = os.path.abspath(os.path.join(base_dir, item))
        if full_path not in valid_plugin_paths:
            try:
                if os.path.isdir(full_path):
                    shutil.rmtree(full_path)
                else:
                    os.remove(full_path)
                print(f"[x] Deleted leftover: {full_path}")
            except Exception as e:
                print(f"[!] Failed to delete {full_path}: {e}")


def download_file(url: str, args, progress: Progress, task_id: int):
    filename = os.path.basename(url)
    filepath = os.path.join(args.download_dir, filename)

    if os.path.exists(filepath):
        if args.delete_existing:
            try:
                os.remove(filepath)
                print(f"[+] Deleted existing: {filename}")
            except Exception as e:
                print(f"[!] Failed to delete {filename}: {e}")
                return None
        else:
            print(f"[+] Already exists: {filename}")
            return filepath

    try:
        os.makedirs(args.download_dir, exist_ok=True)
        with requests.get(url, stream=True, timeout=30) as r:
            r.raise_for_status()
            total_size = int(r.headers.get('content-length', 0))

            progress.update(task_id, total=total_size)

            with open(filepath, 'wb') as f:
                for chunk in r.iter_content(chunk_size=8192):
                    if chunk:
                        f.write(chunk)
                        progress.update(task_id, advance=len(chunk))

        return filepath

    except Exception as e:
        print(f"[!] Failed to download {filename}: {e}")
        if os.path.exists(filepath):
            os.remove(filepath)
        return None


def main():
    parser = argparse.ArgumentParser(description="Addons downloader for the game EIODE")
    parser.add_argument("--workers", type=int, default=DEFAULT_WORKERS)
    parser.add_argument("--urls-file", default=DEFAULT_URLS_FILE)
    parser.add_argument("--download-dir", default=DEFAULT_DOWNLOAD_DIR)
    parser.add_argument("--extract", action="store_true")
    parser.add_argument("--delete-existing", action="store_true")
    args = parser.parse_args()

    if not os.path.exists(args.urls_file):
        print(f"[!] Error: '{args.urls_file}' not found.")
        return

    with open(args.urls_file, 'r') as f:
        urls = [line.strip() for line in f if line.strip()]

    if not urls:
        print(f"[!] No URLs found in '{args.urls_file}'.")
        return

    print(f"[*] Downloading {len(urls)} files with {args.workers} workers...")

    downloaded_files = []

    with Progress(
        TextColumn("[bold blue]{task.fields[filename]}", justify="right"),
        BarColumn(),
        "[progress.percentage]{task.percentage:>3.1f}%",
        DownloadColumn(),
        TransferSpeedColumn(),
        TimeRemainingColumn(),
    ) as progress:

        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            futures = []
            for url in urls:
                filename = os.path.basename(url)
                task_id = progress.add_task("download", filename=filename, start=False)
                future = executor.submit(download_file, url, args, progress, task_id)
                futures.append((future, task_id))

            for future, task_id in futures:
                progress.start_task(task_id)
                result = future.result()
                if result:
                    downloaded_files.append(result)

    if args.extract and downloaded_files:
        print(f"[*] Extracting files...")
        for f in downloaded_files:
            extract_file(f, os.path.dirname(f))

    if args.extract:
        print(f"[*] Searching for plugin.cfg or .gdextension...")
        plugin_dirs = find_plugin_dirs(args.download_dir)

        if plugin_dirs:
            moved_paths = move_plugin_dirs(plugin_dirs, args.download_dir)
            cleanup_non_plugins(args.download_dir, moved_paths)
        else:
            print("[i] No plugin.cfg or .gdextension found in extracted files.")

    print("[+++] All operations complete!")


if __name__ == "__main__":
    main()
