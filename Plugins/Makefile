SUBDIRS = \
	Badness \
	Best \
	ClassDB \
	Correctness \
	Crop \
	Desaturate \
	Edge \
	Euclid \
	FFT \
	Flip \
	Haar \
	Histogram \
	HistogramView \
	ImageView \
	L0Mod \
	Mirror \
	Multiplier \
	NewSourceImages \
	PC-C1 \
	PC-C2 \
	Random \
	Resize \
	ResizeWavelet \
	ResultData \
	RGBSplit \
	SimpleDB \
	SimpleResultView \
	X-of-Y

all: $(SUBDIRS)

.PHONY: $(SUBDIRS)
$(SUBDIRS):
	+make -C $@

clean: $(SUBDIRS:%=%-clean)

$(SUBDIRS:%=%-clean):
	+make -C $(@:%-clean=%) clean
