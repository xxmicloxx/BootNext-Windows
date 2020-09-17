# BootNext-Windows
A tool allowing for rebooting directly to your target OS. This repo contains the macOS client.

## About
This tool allows you to reboot to your target OS without having to click through your bootloader/UEFI.

This is made possible by the EFI application in BootNext-EFI. This is just a client controlling said application. You can install this on your Windows machine and use it to boot your desired OS without having to click through your UEFI or rEFInd. This tool can also install BootNext to your target partition. It does not, however, add BootNext to your EFI boot order. You have to do that yourself at this point.

## Compilation
This tool can be compiled using Visual Studio 2019 (and probably later versions). Just open the solution file and click build.
