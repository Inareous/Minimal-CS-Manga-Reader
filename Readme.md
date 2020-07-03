# Minimal CS Manga Reader

A minimal program for your daily use of reading manga continously in one page. An alternative to the popular DomDomSoft Manga Reader.

![Demo](https://cdn.discordapp.com/attachments/578057213084434433/668406622632280094/unknown.png)

## Features

* Continous-scrolling one page reader.
* Support chapter as folder and archive file (rar, zip, cbz, cbr, tar, and 7zip)*.
* Fast zooming capability.
* Faster performance.
* Configurable (Scrolling speed, background, margin per images, etc).

<sup>\* 7zip performance is significantly slower compared to other archive types</sup>
## Requirement (Build)

.Net Core 3.1 or later.

## How To Use

* Download and extract the latest release build at [/Releases](https://github.com/Inareous/Minimal-CS-Manga-Reader/releases).
* Open Minimal CS Manga Reader.exe

## Integrate With Other Programs

* Enable/check "Integrate Minimal CS Manga Reader with File Explorer Context" in Setting to integrate MCS Manga Reader with Windows Explorer, adding a context menu whenever you right-click on a folder. 
* If you are using [Free Manga Downloader](https://github.com/fmd-project-team) or any other program that support opening app with custom parameter, target to "Minimal CS Manga Reader.exe" in the extracted folder, then add "-path=%PATH%" as parameter (%PATH% is the folder path pattern of the manga).

## Known Issues/Bugs

* Application Startup is slow, can take up to 1-3 second or more.
* MCS Manga Reader consume a lot of memory when opening chapter with > 150 images, this is because all of those images are loaded instantly. I've been mitigating it by using 16bppBRG565 to display the images. Still looking for how to implement Mapping Virtualization to reduce memory footprint further.

## 3rd Party Libraries Used

* [ReactiveUI & ReactiveUI.Events](https://github.com/reactiveui/reactiveui)
* [PropertyChanged.Fody](https://github.com/Fody/PropertyChanged)
* [SharpCompress](https://github.com/adamhathcock/sharpcompress)
* [Material Design In Xaml](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
* [MahApps](https://github.com/MahApps/MahApps.Metro)

## License

MIT
