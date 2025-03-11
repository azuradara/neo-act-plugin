try {
  $VS_PATH = &"${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -property installationPath
  if (-not $VS_PATH) {
      Write-Output "Error: Visual Studio not found!"
      exit 1
  }
  
  $env:Path += ";$VS_PATH\MSBuild\Current\Bin"
  $env:Path += ";$VS_PATH\Common7\IDE"

  if ( -not (Test-Path "$VS_PATH")) {
      Write-Output "Error: VS_PATH isn't set correctly! Update the variable in build.ps1 for your system."
      Write-Output "... or implement it properly with vswhere and submit a PR. (Please)"
      exit 1
  }

  if ( -not (Test-Path "Thirdparty\ACT\Advanced Combat Tracker.exe" )) {
      Write-Output 'Error: Please run tools\fetch_deps.py'
      exit 1
  }

  $ENV:PATH = "$VS_PATH\MSBuild\Current\Bin;${ENV:PATH}";
  if (Test-Path "C:\Program Files\7-Zip\7z.exe") {
      $ENV:PATH = "C:\Program Files\7-Zip;${ENV:PATH}";
  }

  if ( -not (Test-Path .\NeoActPlugin.Updater\Resources\libcurl.dll)) {
      Write-Output "==> Building cURL..."

      mkdir .\NeoActPlugin.Updater\Resources
      Set-Location Thirdparty\curl\winbuild

      Write-Output "@call `"$VS_PATH\VC\Auxiliary\Build\vcvarsall.bat`" amd64"           | Out-File -Encoding ascii tmp_build.bat
      Write-Output "nmake /f Makefile.vc mode=dll VC=16 GEN_PDB=no DEBUG=no MACHINE=x64" | Out-File -Encoding ascii -Append tmp_build.bat
      Write-Output "@call `"$VS_PATH\VC\Auxiliary\Build\vcvarsall.bat`" x86"             | Out-File -Encoding ascii -Append tmp_build.bat
      Write-Output "nmake /f Makefile.vc mode=dll VC=16 GEN_PDB=no DEBUG=no MACHINE=x86" | Out-File -Encoding ascii -Append tmp_build.bat

      cmd "/c" "tmp_build.bat"
      Start-Sleep 3
      Remove-Item tmp_build.bat

      Set-Location ..\builds
      Copy-Item .\libcurl-vc16-x64-release-dll-ipv6-sspi-winssl\bin\libcurl.dll ..\..\..\NeoActPlugin.Updater\Resources\libcurl-x64.dll
      Copy-Item .\libcurl-vc16-x86-release-dll-ipv6-sspi-winssl\bin\libcurl.dll ..\..\..\NeoActPlugin.Updater\Resources\libcurl.dll

      Set-Location ..\..\..
  }

  Write-Output "==> Building..."

  msbuild -p:Configuration=Release -p:Platform=x64 "NeoActPlugin.sln" -t:Restore
  msbuild -p:Configuration=Release -p:Platform=x64 "NeoActPlugin.sln"
  if (-not $?) { exit 1 }

  Write-Output "==> Building archive..."

  Set-Location out\Release

  if (Test-Path NeoActPlugin) { Remove-Item -Recurse NeoActPlugin }
  mkdir NeoActPlugin\libs

  Copy-Item @("NeoActPlugin.dll", "NeoActPlugin.dll.config", "README.md", "LICENSE") NeoActPlugin
  # Copy-Item -Recurse libs\resources NeoActPlugin
  Copy-Item -Recurse libs\*.dll NeoActPlugin\libs
  # Remove-Item NeoActPlugin\libs\CefSharp.*

  $text = [System.IO.File]::ReadAllText("$PWD\..\..\NeoActPlugin\Properties\AssemblyInfo.cs");
  $regex = [regex]::New('\[assembly: AssemblyVersion\("([0-9]+\.[0-9]+\.[0-9]+)\.[0-9]+"\)');
  $m = $regex.Match($text);

  if (-not $m) {
      Write-Output "Error: Version number not found in the AssemblyInfo.cs!"
      exit 1
  }

  $version = $m.Groups[1]
  $archive = "..\neo-act-plugin-v$version.7z"

  if (Test-Path $archive) { Remove-Item $archive }
  Set-Location NeoActPlugin
  7z a ..\$archive .
  Set-Location ..

  $archive = "..\neo-act-plugin-v$version.zip"

  if (Test-Path $archive) { Remove-Item $archive }
  7z a $archive NeoActPlugin

  Set-Location ..\..
} catch {
  Write-Error $Error[0]
}
