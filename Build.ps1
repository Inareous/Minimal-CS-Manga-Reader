param([string]$buildtfm = 'all')

$configuration = 'Release'
$project= "Minimal CS Manga Reader"

function Build-NetCore {
	param([string]$arch)

	Write-Host "Building .NET Core $arch binaries"

	dotnet publish -v:m $project\$project.csproj -c $configuration /p:PublishProfile=`"$project\Properties\PublishProfiles\$arch.pubxml`"
	Copy-Item "$project\$project.ico" "$project\Release\$project $arch"

	if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

$buildCoreX86 = $buildtfm -eq 'all' -or $buildtfm -eq 'x86'
$buildCoreX64 = $buildtfm -eq 'all' -or $buildtfm -eq 'x64'

if ($buildCoreX86) {
	Build-NetCore x86
}

if ($buildCoreX64) {
	Build-NetCore x64
}