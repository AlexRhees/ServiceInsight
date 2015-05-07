---
permalink: /serviceinsight.txt
---

[
  {% for release in site.github.releases %}
  {
  	tag: "{{ release.tag_name }}",
  	release: "{{ release.html_url }}",
  	published: "{{ release.published_at }}",
  	assets: [
  		{% for asset in release.assets %}
  		{
  			name: "{{ asset.name }}",
  			size: {{ asset.size }},
  			download: "{{ asset.browser_download_url }}"
  		}
  		{% endfor %}
  	]
  },
  {% endfor %}
]