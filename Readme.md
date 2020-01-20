# Minimal CS Manga Reader

A minimal program for your daily use of reading manga continously in one page. An alternative to the popular DomDomSoft Manga Reader.

**_MCS Manga Reader is not a stand-alone program_**, please refer to ``How To Use`` to integrate it with other programs.

![Demo](https://cdn.discordapp.com/attachments/578057213084434433/668406622632280094/unknown.png)

## Features

* Continous-scrolling one page reader.
* Support chapter as folder, rar, zip, cbz, and cbr.
* Fast zooming capability.
* Faster performance.
* Configurable (Scrolling speed, background, margin per images, etc).

## Requirement (Build)

.Net Core 3.1 or later.

## How To Use

* Download and extract the latest release build at [/Releases](https://github.com/Inareous/Minimal-CS-Manga-Reader/releases).
* If you are going to integrate ReaderStylet with Windows Explorer, run IntegrateContext.exe as administrator and follow the instruction in the program.
* If you are using [Free Manga Downloader](https://github.com/riderkick/FMD) or any other program that support opening app with custom parameter, target ReaderStylet.exe and add "-path=%PATH%" as parameter, where %PATH% is the folder path pattern of the manga.

## Known Issues/Bugs

* MCS Manga Reader consume a lot of memory when opening chapter with > 150 images, this is because all of those images are loaded instantly. I've been mitigating it by using 16bppBRG565 to display the images. Still looking for how to implement Mapping Virtualization to reduce memory footprint further.

## 3rd Party Libraries Used

* [ReactiveUI & ReactiveUI.Events](https://github.com/reactiveui/reactiveui)
* [PropertyChanged.Fody](https://github.com/Fody/PropertyChanged)
* [SharpCompress](https://github.com/adamhathcock/sharpcompress)
* [Material Design In Xaml](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
* [MahApps](https://github.com/MahApps/MahApps.Metro)

## License

MIT
