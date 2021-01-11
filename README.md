# IPS Resource Updater Action

This action will update a resource on IPS board via the REST API.

## Inputs

//todo

## Example usage

```yml
uses: actions/invision-resource-updater-action@v1
with:
  ips_url: '<ips-url>'
  api_key: '<ips-api-key>'
  resource_id: 12
  file_url: '<url-here>'
  version: '1.0.0'
  changelog: ''
  save_previous: false
```