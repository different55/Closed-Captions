#!/usr/bin/env fish
argparse 'd/domain=' -- $argv
or return

set -l domain "survival"
set -ql _flat_domain
and set domain $_flat_domain

set -l asset_directory "/Applications/Vintage Story.app/assets/"$domain"/sounds/"
set -l data_file "ClosedCaptions/assets/captions/captions/"$domain".json"

stat $asset_directory >/dev/null
or return
stat $data_file >/dev/null
or return

set -l keys (find $asset_directory -iname "*.ogg" | string replace $asset_directory "" | string replace ".ogg" "")
for key in $keys
	# Reformat key to strip numbers.
	set clean_key (string replace -r '[/\d_-]+$' "" $key)
	if not jq -e '.["'$clean_key'"]' $data_file >/dev/null
		echo $clean_key is missing.
	end
end | sort | uniq