# ICE Automation

A command line application to bach process image stitching using the marvellous [Image Compose Editor (ICE)](https://www.microsoft.com/en-us/research/product/computational-photography-applications/image-composite-editor).

# Instructions

1) recommended: copy the batch file ICEAutomation.bat to c:\Windows or some folder in system Path.
2) open a command line and move to the folder where your images are
3) execute 
- "ICEAutomation compose [file1] [file2] [file3...]" to stitch those files
- "ICEAutomation process" to process all *.JPG files in current folder in groups of 3
- "ICEAutomation process [num]" to process all *.JPG files in current folder in groups of [num]
- "ICEAutomation process [num] [ext]" to process all files with extension [ext] in current folder in groups of [num]
- "ICEAutomation process [num] [ext] [folder]" to process all files with extension [ext] in [folder] in groups of [num]

# Warning

The application uses button labels to automate ICE. Depending of your environment this names can change (for example "Save" button).
You can configure the button labels in your ICE in app.config.

