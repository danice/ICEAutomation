# ICE Automation

A command line application to bach process image stitching using the marvellous [Image Compose Editor (ICE)](https://www.microsoft.com/en-us/research/product/computational-photography-applications/image-composite-editor).

# Build
Now the project has moved to netcoreapp3.1. I recommend you to use VS Code to work with it, but only .net core 3.1 SDK is required. Follow this:
1) install [dot.net core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1)
2) open a cmd, move to your <projects> folder and execute:
```
> git clone https://github.com/danice/ICEAutomation.git
> cd ICEAutomation
> dotnet build
```
You will found the compiled files in <projects>\ICEAutomation\src\bin\Debug\netcoreapp3.1
Next adjust the ICEAutomation.bat to point to this folder. Then copy the batch file to c:\Windows or some folder in system Path so you can execute the application from any folder.


# Instructions

1) open a command line and move to the folder where your images are
2) execute 
- "ICEAutomation compose [file1] [file2] [file3...]" to stitch those files
- "ICEAutomation process" to process all *.JPG files in current folder in groups of 3
- "ICEAutomation process [num]" to process all *.JPG files in current folder in groups of [num]
- "ICEAutomation process [num] [ext]" to process all files with extension [ext] in current folder in groups of [num]
- "ICEAutomation process [num] [ext] [folder]" to process all files with extension [ext] in [folder] in groups of [num]

Options:
  - --motion: to specify Camera motion type. Default: autoDetect. Possible values: autoDetect , planarMotion, planarMotionWithSkew, planarMotionWithPerspective, rotatingMotion]
  - --save: saves stich processing file

# Warning

The application uses button labels to automate ICE. Depending of your environment this names can change (for example "Save" button).
You can configure the button labels in your ICE in app.config. 

The processed files will be copied in the last folder used by ICE. So I recommend firt executing manually a stich to select the destination folder.

