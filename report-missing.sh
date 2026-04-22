#!/usr/bin/env fish
argparse 'd/domain=' 'l/language=' -- $argv
or return

set -l language "en"
set -ql _flag_language
and set language $_flag_language

set -l domain "survival"
set -ql _flag_domain
and set domain $_flag_domain

set -l asset_directory "/Applications/Vintage Story.app/assets/"$domain"/sounds/"
set -l lang_file "ClosedCaptions/assets/captions/lang/"$language".json"
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
		echo $clean_key" has no metadata in "$domain".json"
	end
	if not jq -e '.["captions:'$clean_key'"]' $lang_file >/dev/null
		echo $clean_key" has no language data in "$language".json"
	end
end | sort | uniq