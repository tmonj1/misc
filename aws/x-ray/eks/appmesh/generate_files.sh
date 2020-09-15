#!/bin/bash

# exit if an error occurs.
set -e

#
# repalcement list
#
rlist=('{{CLUSTER_NAME}} => app')
rlist+=('{{APP_NAMESPACE}} => app-ns')
rlist+=('{{MESH_NAME}} => my-mesh')
rlist+=('{{SUBNET_1a}} => subnet-058ecd35d42d6516a')
rlist+=('{{SUBNET_1c}} => subnet-068a2568991910099')

#
# global variables
#

# program name
PROGNAME=$(basename $0)

# output directory
out=/dev/stdout

# setting for whether use envsubst or not
subst_env=no

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
  echo "    -e, --envsubst:"
  echo "      substitute environment variables (in the format of ${ENVIRONMENT_VARIABLE}) of the files"
  echo "      which end with '.env.template.yaml' or '.env.template.json'."
  echo ""
  echo "    -o, --output:"
  echo "      output directory. The default is stdout (/dev/stdout)."
  echo ""
  echo "    -l, --show-list:"
  echo "      show replacement keywords."
  echo ""
  echo "  Descrition:"
  echo "    This command replaces all placeholder strings ({{place_holder}}) with replcer strings for files"
  echo "    which end with '.template.yaml' or '.template.json. All other files are ignored."
  echo ""
  echo "    If the output directory is specified with '-o' option, the processed files are saved in that "
  echo "    direcotry. The '.template' part of the filenames is removed at the same time ('test.temlate.yaml"
  echo "    is renamed to 'test.yaml', for example)."
  echo ""
  echo "    If '-e' option is specified, environment variabled embedded in the files are substituted, too."
  echo "    The '.env.template' part of the file name is removed at the same time."
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
check_required_cmd () {
  set +e
  local jqpath=$(which "$1")
  set -e
  if [[ -z "$jqpath" ]]; then
    echo 'ng'
  fi
  echo "ok"
}

# convert all templates to yaml/json files
process_templates () {

  # create sed commands from replacement list
  local sed_cmds=$(IFS=";"; echo "${rlist[*]}" | sed -e 's|^|s/|; s|;|/g;s/|g; s| *=> *|/|g; s|$|/g|')

  # use envsubst if "-e" option is set
  local cat_cmd
  if [[ $subst_env = "yes" ]]; then
    cat_cmd="envsubst"
  else
    cat_cmd="cat"
  fi

  # for all templates
  for tmplfile in "$@"
  do
    # process files whose filename end with ".template.yaml" or "env.template.yaml" if '-e' specified
    if [[ ( $subst_env = "yes" && "${tmplfile}" =~ (^.+).env.template.(yaml|json)$ ) ]]; then
      cat_cmd="envsubst"
    elif [[ "${tmplfile}" =~ (^.+).template.(yaml|json)$ ]]; then
      cat_cmd="cat"
    else
      continue;
    fi

    # remove '.template' part from filename
    local resultfile="${BASH_REMATCH[1]}.${BASH_REMATCH[2]}"
  
    # replace keywords, plus substitute env variables if '-e' is specified
    if [[ $out = "/dev/stdout" ]]; then
      $cat_cmd < "${tmplfile}" |sed -e "${sed_cmds}"
    else
      $cat_cmd < "${tmplfile}" |sed -e "${sed_cmds}" > "$out/${resultfile}"
      echo "${tmplfile} => ${resultfile} ... done"
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
    -e | --envsubst)
      if [[ ! $(check_required_cmd "envsubst") = "ok" ]]; then
        echo "\"envsubst\" is required for \"$OPT\" option."
        exit 1
      fi
      subst_env=yes
      shift 1
      ;;
#    -c | --config)
#      if [[ -z "$2" ]] || [[ "$2" =~ ^-+ ]]; then
#        echo "$PROGNAME: option requires an argument -- $1" 1>&2
#        exit 1
#      fi
#      config_file=$2
#      shift 2
#      ;;
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

# process all templates and emit resulting yaml files to the output directory
process_templates ${param[*]}

exit 0
