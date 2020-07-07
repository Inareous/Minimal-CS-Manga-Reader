# Minimal CS Manga Reader

A minimal program for your daily use of reading manga continously in one page. An alternative to the popular DomDomSoft Manga Reader.

![Demo](https://user-images.githubusercontent.com/18087264/86542571-e93b2100-bf40-11ea-9823-6546fb2cc2c5.PNG)

## Features

* Continous-scrolling one page reader.
* Support chapter as folder and archive file (rar, zip, cbz, cbr, tar, and 7zip*).
* Fast zooming capability.
* Faster performance.
* Configurable (Scrolling speed, background, margin per images, etc).

<sup>\* 7zip performance is significantly slower compared to other archive types</sup>

## How To Use

* **Required** : Download and Install [.NET Core 3.1 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet-core/3.1) for your OS architecture.
* Download and extract the latest release build at [/Releases](https://github.com/Inareous/Minimal-CS-Manga-Reader/releases).
* Open Minimal CS Manga Reader.exe

## Integrate With Other Programs

* Enable/check "Integrate Minimal CS Manga Reader with File Explorer Context" in Setting to integrate MCS Manga Reader with Windows Explorer, adding a context menu whenever you right-click on a folder. 
* If you are using [Free Manga Downloader](https://github.com/fmd-project-team) or any other program that support opening app with custom parameter, target to "Minimal CS Manga Reader.exe" in the extracted folder, then add "%PATH%" as parameter (%PATH% is the folder path pattern of the manga).


## Build Requirement

.NET Core 3.1 or later.

## Known Issues/Bugs

* Application Startup is slow, can take up to 1-3 second or more ([_reference_](https://github.com/dotnet/runtime/issues/13339)).

## 3rd Party Libraries Used

* [ReactiveUI & ReactiveUI.Events](https://github.com/reactiveui/reactiveui)
* [PropertyChanged.Fody](https://github.com/Fody/PropertyChanged)
* [SharpCompress](https://github.com/adamhathcock/sharpcompress)
* [Material Design In Xaml](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit)
* [MahApps](https://github.com/MahApps/MahApps.Metro)

## License

MIT
