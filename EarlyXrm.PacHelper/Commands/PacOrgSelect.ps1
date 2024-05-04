$devFilter = '*' # 'Blah-DEV?'

write-host "Retrieving org info associated with current auth profile"

$whoJob = Start-Job { pac org who --json }
$orgsJob = Start-Job { pac admin list --json }

Wait-Job -id $whoJob.Id, $orgsJob.Id | Out-Null

$whoJobResult = Receive-Job $whoJob.Id
$orgsJobResult = Receive-Job $orgsJob.Id

try {
	$who = ConvertFrom-Json $whoJobResult
	$envs = ConvertFrom-Json $orgsJobResult
	$currentOrg = $who.FriendlyName
} catch {
	write-error $who
	write-error $envs
	write-host "Try re-authenticating via pac auth"
	exit 1
}

$devEnvs = @( $envs | where-object { $_.DisplayName -like $devFilter } )

$selected = 0
for ($i=1; $i -le $devEnvs.length; $i++) {
	$devEnv = $devEnvs[$i-1]
	$displayName = $devEnv.DisplayName
	if ($displayName -eq $currentOrg) {
		$selected = $i
		write-host "[$i] * $displayName"
	} else {
		write-host "[$i]   $displayName"
	}
}

if ($selected -eq 0) {
	$envIndex = Read-Host -Prompt "`r`nChoose target org index and <Enter>"
} else {
	$envIndex = Read-Host -Prompt "`r`nChoose target org index and <Enter> - or just <Enter> for current (*)"
}

if ($envIndex -ne "") {
	pac org select -env $devEnvs[$envIndex-1].EnvironmentUrl
}