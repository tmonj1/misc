#!/bin/bash

# exit if an error occurs.
set -e

PROGNAME=$(basename $0)

# output directory
out=/dev/stdout

# repalcement list
rlist=('{{CLUSTER_NAME}} => app')
rlist+=('{{APP_NAMESPACE}} => $APP_NAMESPACE')
rlist+=('{{MESH_NAME}} => my-mesh')
rlist+=('{{abc}} => ABC')
rlist+=('{{def}} => DEF')

#
# functions
#
show_usage () {
  echo ""
  echo "  Usage:"
  echo "    $PROGNAME [options] [template-files]"
  echo "    $PROGNAME --show-list (show replacement list)"
  echo "    $PROGNAME --help (for help)"
  echo ""
  echo "  Options:"
  echo "    -o, --output:"
  echo "      output directory. The default is stdout (/dev/stdout)."
  echo ""
  echo "    -l, --show-list:"
  echo "      show replacement keywords."
  echo ""
  echo "  Descrition:"
  echo "    1) This command replaces all placeholder strings ({{place_holder}}) with replcer strings for files"
  echo "       which end with '.template.yaml'."
  echo ""
  echo "    2) All yaml files which end with '.yaml' but not end with '.template.yaml' are copied to"
  echo "       the output directory as is." 
  echo ""
  echo "    3) Other files are ignored."
  echo ""
  echo "  Example:"
  echo "    $ $PROGNAME --out yaml app-ns.template.yaml, app-cluster.template.yaml"
  echo "    $ ls ./yaml"
  echo "      ./yaml/app-ns.yaml"
  echo "      ./yaml/app-cluster.yaml"
  echo ""
}

# show replacement list
show_replacement_list () {
  echo "These keywords will be replaced:"
  echo ""
  IFS='
  '; echo "${rlist[*]}" | sed -e 's|^|  |
  # IFS='
  # '; echo "${rlist[*]}" |sed -e 's|^s/|    |;s|/g$||;s|/| => |'
  echo ""
}

# check if output dir exists (if not, create one)
check_outdir () {
  if [ ! -e "$1" ]; then
    if [ ! "$1" = "/dev/stdout" ]; then
      echo "$1 not exsit, create one..." 1>&2
      mkdir "$1"

      if [ ! $? = 0 ]; then
        echo "error: cannot create "$1", exited." 1>&2
        exit 1
      fi
    fi

    echo "$1"
  else
    echo "$1"
  fi
}

# check if jq is installed or not
check_requirements () {
  set +e
  jqpath=$(which jq)
  set -e
  if [[ -z "$jqpath" ]]; then
    echo "This command requires 'jq' installed." 1>&2
    exit 1
  fi
  echo "ok"
}

# convert all templates to yaml files
process_templates_to_yamlfiles () {

  # create sed commands from replacement list
  sed_cmds=$(IFS=";"; echo "${rlist[*]}" | sed -e 's|^|s/|; s|;|/g;s/|g; s| *=> *|/|g; s|$|/g|')

  # for all templates
  for tmplfile in "$@"
  do
    # process files whose filename end with ".template.yaml"
    if [[ "${tmplfile}" =~ (^.+).template.yaml$ ]]; then
      yamlfile="${BASH_REMATCH[1]}.yaml"
      echo "generating a yaml file ${yamlfile} from ${tmplfile}.."
  
      if [[ $out = "/dev/stdout" ]]; then
        cat "${tmplfile}" |sed -e "${sed_cmds}"
        # echo "{{abc}}111{{def}}xyz:" |sed -e "${sed_cmds}"
      else
        cat "${tmplfile}" |sed -e "${sed_cmds}" > "$out/${yamlfile}"
        # echo "{{abc}}111{{def}}xyz:" |sed -e "${sed_cmds}"
      fi
    fi
  done
}

#
# main
#

for OPT in "$@"
do
  case $OPT in
    -h | --help)
      show_usage
      exit 0
      ;;
    -l | --show-list)
      show_replacement_list 
      exit 0
      ;;
    -o | --output)
      if [[ -z "$2" ]] || [[ "$2" =~ ^-+ ]]; then
        echo "$PROGNAME: option requires an argument -- $1" 1>&2
        exit 1
      fi
      out=$(check_outdir "$2")
      shift 2
      ;;
    -c | --config)
      if [[ -z "$2" ]] || [[ "$2" =~ ^-+ ]]; then
        echo "$PROGNAME: option requires an argument -- $1" 1>&2
        exit 1
      fi
      config_file=$2
      shift 2
      ;;
    -- | -)
      shift 1
      param+=( "$@" )
      break
      ;;
    -*)
      echo "$PROGNAME: illegal option -- '$(echo $1 | sed 's/^-*//')'" 1>&2
      exit 1
      ;;
    *)
      if [[ ! -z "$1" ]] && [[ ! "$1" =~ ^-+ ]]; then
        param+=( "$1" )
        shift 1
      fi
      ;;
  esac
done

if [ -z "$param" ]; then
    echo "$PROGNAME: too few arguments" 1>&2
    echo "Use '$PROGNAME --help' for more information." 1>&2
    exit 1
fi

# check requirements (need jq)
# check_requirements

# process all templates and emit resulting yaml files to the output directory
process_templates_to_yamlfiles "$param"

exit 0
