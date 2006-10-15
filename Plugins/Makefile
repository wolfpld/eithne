SUBDIRS = \
	Desaturate \
	Euclid \
	Histogram \
	ImageView \
	L0Mod \
	Multiplier \
	NewSourceImages \
	PC-C1 \
	PC-C2 \
	Random \
	Resize \
	ResultData \
	RGBSplit \
	SimpleDB \
	SimpleResultView

all: $(SUBDIRS)

.PHONY: $(SUBDIRS)
$(SUBDIRS):
	make -C $@

clean: $(SUBDIRS:%=%-clean)

$(SUBDIRS:%=%-clean):
	make -C $(@:%-clean=%) clean
