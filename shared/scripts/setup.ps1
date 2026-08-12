# EXECUTE FROM OUTSIDE SCRIPTS FOLDER

$decision = $Host.UI.PromptForChoice(
	"Make sure you are executing this from outside the scripts folder. Inside the shared folder.",
	"Are you?",
	("&Yes", "&No"),
	1
)

if ($decision -eq 1) {
	exit
}

mkdir ../local-nuget-feed

dotnet nuget add source "$(Resolve-Path ..\local-nuget-feed)" --name FredagsbarNugetFeed
