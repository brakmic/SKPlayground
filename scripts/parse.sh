#!/usr/bin/env bash

# Define patterns
chart_pattern="CHART:"
file_pattern="FILE:"
content_delimiter="#----#"

# Parsing command line arguments
while [[ "$#" -gt 0 ]]; do
    case $1 in
        -f|--file) input_file="$2"; shift ;;
        -o|--output) output_dir="$2"; shift ;;
        *) echo "Unknown parameter passed: $1"; exit 1 ;;
    esac
    shift
done

# Ensure both arguments are provided
if [[ -z $input_file || -z $output_dir ]]; then
    echo "Usage: $0 --file <input-file> --output <output-dir>"
    exit 1
fi

# Initialize variables
current_chart_name="default"
current_chart_dir="$output_dir/$current_chart_name"
current_file=""
writing_file=false

# Read each line of the input file
while IFS= read -r line; do
    # If line starts with CHART, it's indicating a new chart
    if [[ $line == $chart_pattern* ]]; then
        current_chart_name=$(echo $line | cut -d' ' -f2)
        current_chart_dir="$output_dir/$current_chart_name"
        mkdir -p "$current_chart_dir"
        writing_file=false  # Reset writing_file flag
    # If line starts with FILE, it's indicating a new file
    elif [[ $line == $file_pattern* ]]; then
        file_name=$(echo $line | cut -d':' -f2)
        file_dir=$(dirname "$file_name")
        mkdir -p "$current_chart_dir/$file_dir"
        current_file="$current_chart_dir/$file_name"
        > "$current_file"  # Reset the current file
        writing_file=true  # Set writing_file flag
    # If writing_file is set, write the line to the current file
    elif [[ $writing_file == true && -n $current_file ]]; then
        # Skip writing if the line equals the content delimiter
        if [[ $line == $content_delimiter ]]; then
            continue
        fi
        echo "$line" >> "$current_file"
    fi
done < "$input_file"  # Providing the input file to the while loop
