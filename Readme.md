# Minimal CS Manga Reader

A minimal program for your daily use of reading manga continously in one page. An alternative to the popular DomDomSoft Manga Reader.

**_MCS Manga Reader is not a stand-alone program_**, please refer to ``How To Use`` to integrate it with other programs.

ReleaseVersion, Issue

![Demo](https://user-images.githubusercontent.com/18087264/42695427-8a203624-86df-11e8-92c6-d3c9cd6ca1e1.PNG)

# Features

* Continous-scrolling one page reader.
* Support chapter as folder, rar, zip, cbz, and cbr.
* Fast zooming capability.
* Faster performance.
* Configurable (Scrolling speed, background, margin per images, etc).

# Requirement

.Net Framework 4.6.1 or later.

# How To Use

* Download and extract the latest release build at [/Releases](https://github.com/Inareous/ReaderStylet/releases).
* If you are going to integrate ReaderStylet with Windows Explorer, run IntegrateContext.exe as administrator and follow the instruction in the program.
* If you are using [Free Manga Downloader](https://github.com/riderkick/FMD) or any other program that support opening app with custom parameter, target ReaderStylet.exe and add "-path=%PATH%" as parameter, where %PATH% is the folder path pattern of the manga.

# Known Issues/Bugs
* MCS Manga Reader consume a lot of memory when opening chapter with > 150 images, this is because all of those images are loaded instantly. I've been mitigating it by using 16bppBRG565 to display the images. Still looking for how to implement Mapping Virtualization to reduce memory footprint further.
* Image counter will stop working when moving chapter too fast.
* Scrollbar Thumb is hard to see on Light Mode with black background
* There is no way to open another manga/folder. This is by design of MCS and i currently have no intention to change it.

# 3rd Party Libraries Used

* [Stylet](https://github.com/canton7/Stylet)
* [SharpCompress](https://github.com/adamhathcock/sharpcompress)
* [Material Design In Xaml](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
