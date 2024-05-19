param(
	$ItemPath # website.yml file path
)

write-host "pages file path: $ItemPath"

if (Test-Path -Path $ItemPath -PathType Container) {
	write-warning "A valid website.yml file is required!"
	exit 1
}

$directory = [System.IO.Path]::GetDirectoryName($ItemPath)

$settings = Get-Content -Path $ItemPath

$website = @( $settings | ? {$_.Split(":")[0] -eq "adx_websiteid"} )

$val = $website[0].toString().Split(":")[1].Trim()

write-host $val

pac pages upload -p $directory -mv 2