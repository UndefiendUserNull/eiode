# How To Download The Addons

## I. Downloading Addons using `addons_downloader`

1. Download Python, The Latest version should work, make sure to check "Add Python to PATH" during installation

2. Head to the `addons_downloader` folder and run your cmd or PowerShell

3. Run this command `pip install -r requirements.txt`

4. After finishing, Run this command `py main.py --extract` this will download and extract addons for you

5. You're mostly done, but you now have to validate the files

## II. Validating The Addons

Since people think differently, some addon creators may put the addon files directly inside the archive, which is what most people do and the most suitable for our addons downloader, **however**, some others make it inside of an "addons" folder, or any folder actually, so you have to check if ALL the extracted Addons have the `"plugin.gd"` or `"plugin.cfg"` inside of them, if inside the extracted addon folder itself another folder that contains a bunch of stuff, move the files outside that folder and you're Done :D

*P.S: The reason why I did this is to give credit to all the people who created these addons and to reduce the repo size*

## III. Enabling The Addons "Plugins from now"

1. Open the project in Godot

2. Head to Projecet -> Project Settings ... -> Globals

3. Click on the folder icon next to the `Path:`

4. Go to `Addons/cyclops_level_builder` then find `cyclops_global_scene.tscn` and add it

5. **MAKE SURE** to name it "`CyclopsAutoload`", this is key sensitive

6. Head to the Plugins tab next the Globals one

7. Enable all the plugins one by one

8. You're Finally Done :D