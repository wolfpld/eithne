PO = $(wildcard *.po)

LANGS = $(PO:.po=)
MO = $(addsuffix /LC_MESSAGES/eithne.mo,$(LANGS))

all: $(MO)

clean:
	rm -f */LC_MESSAGES/eithne.mo

pl/LC_MESSAGES/eithne.mo: pl.po
	msgfmt -v $< -o $@

update:
	cd .. && xgettext -F --from-code=UTF-8 -o locale/new.po `find | egrep '[:.:](glade|cs)$$' | cut -b 3-`
	msgmerge -v -U pl.po new.po
	rm -f new.po
