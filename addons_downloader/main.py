import os
import argparse
import requests
import zipfile
import tarfile
from concurrent.futures import ThreadPoolExecutor
from tqdm import tqdm

# Default
DEFAULT_URLS_FILE = "ADDONS.txt"
DEFAULT_DOWNLOAD_DIR = "addons"
DEFAULT_WORKERS = 4

def extract_file(filepath: str):
    # Extract compressed files directly in the download directory
    try:
        if filepath.endswith('.zip'):
            with zipfile.ZipFile(filepath, 'r') as zip_ref:
                zip_ref.extractall(os.path.dirname(filepath))
            print(f"[+] Extracted: {os.path.basename(filepath)}")
        elif filepath.endswith('.tar.gz') or filepath.endswith('.tgz'):
            with tarfile.open(filepath, 'r:gz') as tar_ref:
                tar_ref.extractall(os.path.dirname(filepath))
            print(f"[+] Extracted: {os.path.basename(filepath)}")
        else:
            print(f"[i] Skipping extraction (unsupported format): {os.path.basename(filepath)}")
    except Exception as e:
        print(f"[!] Failed to extract {os.path.basename(filepath)}: {e}")


def download_file(url: str):
    filename = url.split("/")[-1]
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
            if args.extract:
                extract_file(filepath)
            return filepath

    
    try:
        with requests.get(url, stream=True, timeout=30) as r:

            r.raise_for_status()
            total_size = int(r.headers.get('content-length', 0))
            
            os.makedirs(args.download_dir, exist_ok=True)

            with open(filepath, 'wb') as f, tqdm(
                desc=filename,
                total=total_size,
                unit='iB',
                unit_scale=True,
                unit_divisor=1024,
            ) as bar:
                
                for chunk in r.iter_content(chunk_size=8192):
                    f.write(chunk)
                    bar.update(len(chunk))

        # print(f"[+] Downloaded: {filename}")
        
        if args.extract:
            extract_file(filepath)

    except Exception as e:
        print(f"\n [-] Failed to download {filename}: {e} \n")

        if os.path.exists(filepath):
            os.remove(filepath)

def main():

    parser = argparse.ArgumentParser(description="Addons downloader for the game EIODE, made by Hazem (UndefiendUserNull)")

    parser.add_argument(
        "--workers", 
        type=int, 
        default=DEFAULT_WORKERS,
        help=f"Number of concurrent downloads (default: {DEFAULT_WORKERS})"
    )

    parser.add_argument(
        "--urls-file",
        default=DEFAULT_URLS_FILE,
        help=f"File containing download URLs (default: {DEFAULT_URLS_FILE})"
    )

    parser.add_argument(
        "--download-dir",
        default=DEFAULT_DOWNLOAD_DIR,
        help=f"Directory to save downloads (default: {DEFAULT_DOWNLOAD_DIR})"
    )

    parser.add_argument(
        "--extract",
        action="store_true",
        help="Automatically extract downloaded archives, or extract existing downloads"
    )
    
    parser.add_argument(
    "--delete-existing",
    action="store_true",
    help="Delete existing files before downloading, helps if 'not a zip file' error happens or when download is corrupted"
    )
    
    global args
    args = parser.parse_args()

    if not os.path.exists(args.urls_file):
        print(f"[!] Error: '{args.urls_file}' not found.")
        return
    
    with open(args.urls_file, 'r') as f:
        urls = [line.strip() for line in f if line.strip()]
    
    if not urls:
        print(f"[!] No URLs found in '{args.urls_file}'.")
        return
    
    # Download phase
    print(f"[*] Downloading {len(urls)} files with {args.workers} workers...")
    downloaded_files = []
    with ThreadPoolExecutor(max_workers=args.workers) as executor:
        results = list(executor.map(download_file, urls))
        downloaded_files = [f for f in results if f is not None]
    
    # Extraction phase
    if args.extract and downloaded_files:
        print(f"\n[*] Extracting {len(downloaded_files)} files with {args.workers} workers...")
        with ThreadPoolExecutor(max_workers=args.workers) as executor:
            list(executor.map(extract_file, downloaded_files))
    
    print("\n[+++] All operations complete!")

if __name__ == "__main__":
    main()