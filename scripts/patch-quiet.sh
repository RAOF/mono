#!/bin/sh

#
# patch-quiet.sh: Shell script to rewrite Makefiles using libtool to be less verbose
#

if [ "$1" = "" ]; then
	echo "Usage: patch-quiet.sh <path to Makefile>"
	exit 1
fi

src=$1

# compile
sed -e 's/^\t$(COMPILE)/\t$(if $(V),,@echo -e "CC\\t$@";) $(COMPILE)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
sed -e 's/^\t$(LTCOMPILE)/\t$(if $(V),,@echo -e "CC\\t$@";) $(LTCOMPILE)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
# link
# automake defines multiple symbols ending with LINK
sed -e 's/\t$(\(.*LINK\))/\t$(if $(V),,@echo -e "LD\\t$@";) $(\1)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
#sed -e 's/LINK = $(LIBTOOL)/LINK = $(if $(V),,@echo -e "LD\\t$@";) $(LIBTOOL)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
# CC
sed -e 's/^\t$(CC)/\t$(if $(V),,@echo -e "CC\\t$@";) $(CC)/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
# mv
sed -e 's/\tmv -f/\t$(if $(V),,@)mv -f/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp
# libtool messages
sed -e 's/\$(LIBTOOL)/$(LIBTOOL) --quiet/g' < $src > $src.tmp && cp $src.tmp $src && rm -f $src.tmp

# FIXME: libtool message which is not silenced by --quiet:
# $echo "copying selected object files to avoid basename conflicts..."