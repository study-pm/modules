# Setup

- [Overview](#overview)
  - [About this file](#about-this-file)
- [Prerequisites](#prerequisites)
  - [Git](#git)
    - [Linux](#linux)
    - [Windows](#windows)
      - [Using installer](#using-installer)
      - [Using winget tool](#using-winget-tool)
- [Fetch code](#fetch-code)
- [Instal MS SQL Server](#instal-ms-sql-server)
- [Install SSMS](#install-ssms)
  - [Unattended install](#unattended-install)

## Overview

### About this file
This file provides detailed setup instructions for developers and maintainers, such as fetching the source code, managing the dependencies, setting up environments, build generation, running tests etc.

> **Note**: Any dependencies added to this project (or modifying it) which affect the running of the code in this git repository must be listed in this file. All developers must ensure that the instructions mentioned in this file are sufficient to enable a new developer to obtain an executable/runnable/working copy of the lastest code in this repository, without involvement from any other human assistance.

## Prerequisites
Before fetching and installing the project you must have the appropriate working environment. The project can be operated on Linux or Windows systems and requires some special software.

### Git
Git is necessary to fetch, commit and deliver the source code. Detailed setup instructions are layed down on the [official git site](https://git-scm.com/downloads).

#### Linux
It is easiest to install Git on Linux using the preferred package manager of your Linux distribution. Go https://git-scm.com/download/linux and follow the setup instructions.

For the latest stable version for your release of Debian/Ubuntu
```
# apt-get install git
```
For Ubuntu, this PPA provides the latest stable upstream Git version
```
# add-apt-repository ppa:git-core/ppa # apt update; apt install git
```

To check the git version use any of the following commands:
- `git --version`
- `git -v`

#### Windows
Follow the setup instructions on https://git-scm.com/download/win.

##### Using installer
You can download git for Windows from installer from https://git-scm.com/download/win. Choose a compatible version. We recommend using Git Bash standalone for Windows.

##### Using winget tool
Install winget tool if you don't already have it, then type this command in command prompt or Powershell.
```ps
$ winget install --id Git.Git -e --source winget
```

## Fetch code
1. Use `git clone` command to fetch the source code from a remote repository. Using `--recurse-submodules` option will automatically initialize and update each submodule in the repository, including nested submodules if any of the submodules in the repository have submodules themselves.

    ```sh
    $ git clone --recurse-submodules  git@gitlab.com:<project-group>/<project-name>.git
    ```

    > ❗ Don't forget to prepend the domain with your account name if you have multiple git accounts:

    ```sh
    $ git clone --recurse-submodules git@dev-pm.git@gitlab.com:<project-group>/<project-name>.git
    ```

2. Change into the project's root folder:

    ```sh
    $ cd <project-name>
    ```

3. To also initialize, fetch and checkout any nested submodules, you can use the foolproof `git submodule update --init --recursive`:

    ```sh
    $ git submodule update --init --recursive
    ```

    > If you want the submodules merged with remote branch automatically, use `--merge` or `--rebase` with `--remote` option. Otherwise all submodules will be in the `DETACHED HEAD` state:

    ```sh
    $ git submodule update --init --recursive --remote --merge
    ```

## Instal MS SQL Server
1. Go to [SQL Server downloads page](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) to download a free specialized edition version:
   - [SQL Server 2022 Developer](https://go.microsoft.com/fwlink/p/?linkid=2215158&clcid=0x409&culture=en-us&country=us) is a full-featured free edition, licensed for use as a development and test database in a non-production environment.
   - [SQL Server 2022 Express](https://go.microsoft.com/fwlink/p/?linkid=2216019&clcid=0x409&culture=en-us&country=us) is a free edition of SQL Server, ideal for development and production for desktop, web, and small server applications.
2. Open the downloaded installer: *SQL2022-SSEI-Dev.exe* or *SQL2022-SSEI-Expr.exe* depending on the chosen version.
3. Select an installation type (e.g. *Basic*), accept Microsoft SQL Server License Terms, choose the installation location (if needed) and click the Install button.
4. On successful installation the server settings will be showed (see example settings bellow):
   - Instance Name: `MSSQLSERVER` (Developer) or `SQLEXPRESS` (Express)
   - SQL Administrator: *`PC-name`*`\`*`user-name`*
   - Features Installed: `SQLENGINE`
   - Version: `16.0.1000.6, RTM`
   - Connection String: `Server=localhost;Database=master;Trusted_Connection=True;` (Developer) or `Server=localhost\SQLEXPRESS;Database=master;Trusted_Connection=True; (Express)`
   - SQL Server Install Log Folder: `C:\Program Files\Microsoft SQL Server\160\Setup Bootstrap\Log\20241028_132046`
   - Installation Media Folder: `C:\SQL2022\Developer_ENU` (Developer) or `C:\SQL2022\Express_ENU` (Express)
   - Installation Resources Folder: `C:\Program Files\Microsoft SQL Server\160\SSEI\Resources`
5. You can also *Connect Now* to directly connect via the server shell or *Install SSMS* from this window.

## Install SSMS
1. Go to [Download SQL Server Management Studio (SSMS) page](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16) and follow the instructions or simply follow this link to [Download SQL Server Management Studio (SSMS) 20.2](https://aka.ms/ssmsfullsetup).
2. Run the downloaded installer (e.g.: *SSMS-Setup-ENU.exe*) and wait for the installation process to complete. If you come across the Microsoft OLE DB Driver error, simply restart your system and try running the installer once more.
3. Open the SQL Server Management Studio. *Connect to Server* window will appear. Choose the *Server name* in the Login section (the name of the insalled MS SQL Server). If you don't see any, check [the respective troubleshooting section](#ssms-server-connection).

### Unattended install
You can install SSMS using PowerShell.

Follow these steps to install SSMS in the background with no GUI prompts.

1. Launch PowerShell with elevated permissions.

2. Type the following command.

   ```ps
    $media_path = "<path where SSMS-Setup-ENU.exe file is located>"
    $install_path = "<root location where all SSMS files will be installed>"
    $params = " /Install /Quiet SSMSInstallRoot=$install_path"

    Start-Process -FilePath $media_path -ArgumentList $params -Wait
    ```

    For example:
    ```ps
    $media_path = "C:\Installers\SSMS-Setup-ENU.exe"
    $install_path = "$env:SystemDrive\SSMSto"
    $params = "/Install /Quiet SSMSInstallRoot=`"$install_path`""

    Start-Process -FilePath $media_path -ArgumentList $params -Wait
    ```

    You can also pass `/Passive` instead of `/Quiet` to see the setup UI.

3. If all goes well, you can see SSMS installed at *%systemdrive%\SSMSto\Common7\IDE\Ssms.exe* based on the example. If something went wrong, you could inspect the error code returned and review the log file in `%TEMP%\SSMSSetup`.
