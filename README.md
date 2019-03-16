# RoughGrep

[![Build Status](https://dev.azure.com/ville0567/ville/_apis/build/status/vivainio.RoughGrep?branchName=master)](https://dev.azure.com/ville0567/ville/_build/latest?definitionId=5&branchName=master)

Fast, brutalist UI on top of RipGrep

"You RipGrep, so why not RoughGrep?" -Anon, 2018

![Screenshot](https://user-images.githubusercontent.com/557579/42448089-b4c3842a-8384-11e8-8a20-f1924045a522.png)


## Installation

Grab it from [Releases](https://github.com/vivainio/RoughGrep/releases) and unzip somewhere. Works best when you can
launch it from command line.

If you want to install RoughGrep for Explorer right click context menu, run "rgg --install" as administator.

## Requirements

- RipGrep (rg.exe) on PATH. "choco install ripgrep"
- Microsoft Windows (R)
- .NET Framework 4.7.2 (because Windows Forms font rendering is broken on the old ones)
- VSCode launcher ("code") on PATH

## Usage

- Go to the directory you want to search from in prompt of your choice.
- Run rgg.exe. You may also add ripgrep command line arguments, e.g. `rgg -g *.fs` to restrict the search to glob pattern
- Enter the search string in the box and press enter.
- Navigate the results. When you find interesting result you can:
  - Press `space` to open the file at line in preview window
  - Press `ENTER` to open it in VSCode
  - Press `p` to open the parent project DIRECTORY in vscode. RoughGrep takes the best guess on what that might be (e.g. finds .csproj).
  - Press `n` to create a note from current line (and file) to scratchpad window. If there is selection, it's
    appended instead of just the line.
  - Press `g` to view git history for the file in `gitk`.
  - Press `d` to open containing folder in Windows file explorer.

- You can modify RipGrep command line arguments after launch by using the ComboBox from the Status Bar.
- If you want to *find file names* instead of finding contents, use the `--files` command line argument.
  Then, the "text to search for" inputs becomes the list of glob patterns instead.
- Configurable in Scheme! Undocumented, but [you get the idea](https://github.com/vivainio/RoughGrep/blob/master/RoughGrep/RoughGrep.ss).

## Acknowledgements

Credit for the blazing fast UI performance goes to [ScintillaNet](https://github.com/jacobslusser/ScintillaNET) editor component.

## License

MIT
