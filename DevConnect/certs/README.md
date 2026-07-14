# This folder holds local/corporate root CA certificates (*.crt) that need to be
# trusted inside the Docker build (e.g. Cisco Umbrella SSL inspection on the
# corporate network). The .crt files are gitignored and machine/network-specific.
#
# To regenerate the corporate CA on Windows (behind Cisco Umbrella):
#   $c = Get-ChildItem Cert:\LocalMachine\Root, Cert:\CurrentUser\Root |
#        Where-Object { $_.Subject -like '*Cisco Umbrella Root CA*' } | Select-Object -First 1
#   $b = [Convert]::ToBase64String($c.RawData, 'InsertLineBreaks')
#   "-----BEGIN CERTIFICATE-----`n$b`n-----END CERTIFICATE-----`n" |
#        Set-Content -Path .\corporate-root-ca.crt -Encoding ascii
#
# The Dockerfile copies this whole folder into /usr/local/share/ca-certificates/
# and runs update-ca-certificates. If no .crt is present, the build still works.
