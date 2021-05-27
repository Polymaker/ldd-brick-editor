
function FindMsbuid {
	Param ([string]$searchPath)

	$msbuildFile = Get-ChildItem -Path $searchPath -Filter MSBuild.exe -Recurse| Select-Object -First 1
	return $msbuildFile
}

Write ""
Write "Preparing Brick Editor Release"
Write ""

$solutionPath = "..\sources\LDD.BrickEditor.sln"
$setupProjectPath = "..\sources\BrickEditor.Setup\BrickEditor.Setup.wixproj"
$setupBuildPath = "..\sources\LDD.BrickEditor.Setup\bin\Release"
$publishFolder = "..\publish"

$setupInfoPath = "..\sources\LDD.BrickEditor.Setup\Product.wxs"
[XML]$appDetails = Get-Content $setupInfoPath
$appVersion = $appDetails.Wix.Product.Version
Write "Current application version: ${appVersion}"

$choice = Read-Host -Prompt 'Do you want to change the version? [Y/N]'

if ($choice -eq "Y") {
	$newVersion = Read-Host -Prompt 'Enter new version'
	if ($newVersion -ne "") {
		
		Write "Changing project and setup version..."
		$assemblyInfoPath = "..\sources\LDD.BrickEditor\Properties\AssemblyInfo.cs"
		$appVersion = $newVersion
		(Get-Content $assemblyInfoPath) -replace '(^\[assembly: Assembly(?:File)?Version\(")(.+?)("\))', "`${1}$newVersion`${3}" | Out-File "..\sources\LDD.BrickEditor\Properties\AssemblyInfo.cs"
		$appDetails.Wix.Product.Version = "$newVersion"
		$appDetails.Save((Resolve-Path $setupInfoPath))
	}
}


[XML]$appDetails = Get-Content "..\sources\LDD.BrickEditor.Setup\BrickEditor.Setup.wixproj"
$setupFilename = $appDetails.Project.PropertyGroup[0].OutputName
#Write $setupFilename


$msbuild = FindMsbuid "C:\Program Files (x86)\Microsoft Visual Studio\2019"
$msbuildPath = $msbuild.FullName

if (!(Test-Path $msbuildPath)) {
	#Write "msbuild not found"
	$msbuild = FindMsbuid "C:\Program Files (x86)\Microsoft Visual Studio\2017"
	$msbuildPath = $msbuild.FullName
}


if (!(Test-Path $msbuildPath)) {
	Write "msbuild could not be found!" -foregroundcolor red
	exit
}

Write ""
Write-Host "Building the solution for platform: 32-bit" -foregroundcolor green
& "$($msbuildPath)" "$($solutionPath)" /p:Configuration=Release /p:Platform=x86 /t:Build /verbosity:quiet /noconlog

Write ""
Write-Host "Building the solution for platform: 64-bit" -foregroundcolor green
& "$($msbuildPath)" "$($solutionPath)" /p:Configuration=Release /p:Platform=x64 /t:Build /verbosity:quiet /noconlog


function RenameAndCopySetup {
	Param ([string]$releaseDir, [string]$publishDir, [string]$setupName, [string]$setupVersion, [string]$platform)
	
	$setupPath = "$releaseDir\$platform\$setupName.msi"
	if (Test-Path $setupPath) {
		Write "Renaming and copying ${platform} setup to publish folder..."
		$newName = "ldd-brick-editor_${setupVersion}_${platform}.msi"
		Rename-Item -Path $setupPath -NewName $newName
		$setupPath = "$releaseDir\$platform\$newName"
		#Remove-Item "$publishDir\*${platform}.msi"
		Copy-Item $setupPath -Destination $publishDir -force
	}else {
	  Write-Host "Could not find $platform setup!" -foregroundcolor yellow
	}
	
}

RenameAndCopySetup $setupBuildPath $publishFolder $setupFilename $appVersion "x86"
RenameAndCopySetup $setupBuildPath $publishFolder $setupFilename $appVersion "x64"


