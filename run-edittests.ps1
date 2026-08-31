# Runs Unity's EditMode tests from the command line.
#
# Notes (learned the hard way while setting this up):
# - Do NOT pass -quit alongside -runTests: the test runner quits on its own
#   once finished, and -quit makes Unity exit before tests actually run.
# - Results/logs must be written outside the project's Temp/ folder -- Unity
#   clears Temp/ on a clean shutdown, which deletes them before you can read them.
# - The initial Unity.exe process returns almost immediately and hands off to
#   a child process that does the real work, so waiting on the call itself
#   isn't enough -- poll for the results file instead.

$unityExe = "C:\Program Files\Unity\Hub\Editor\6000.5.1f1\Editor\Unity.exe"
$projectPath = $PSScriptRoot
$resultsPath = Join-Path $env:TEMP "UntitledUnityMobileGame-EditModeTestResults.xml"
$logPath = Join-Path $env:TEMP "UntitledUnityMobileGame-EditModeTestRun.log"

Remove-Item $resultsPath -ErrorAction SilentlyContinue

& $unityExe -batchmode -nographics `
    -projectPath $projectPath `
    -runTests -testPlatform EditMode `
    -testResults $resultsPath `
    -logFile $logPath

$timeoutSeconds = 300
$waited = 0
while (-not (Test-Path $resultsPath) -and (Get-Process -Name "Unity" -ErrorAction SilentlyContinue) -and $waited -lt $timeoutSeconds) {
    Start-Sleep -Seconds 2
    $waited += 2
}

Write-Host "Log: $logPath"
if (Test-Path $resultsPath) {
    [xml]$results = Get-Content $resultsPath
    $run = $results.'test-run'
    Write-Host "Result: $($run.result) - $($run.passed)/$($run.total) passed, $($run.failed) failed"
    Write-Host "Results: $resultsPath"
} else {
    Write-Host "No results file was produced -- check the log (likely a compile error)."
}
