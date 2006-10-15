MCS = mcs

TARGET = NewSourceImages.dll

SRC = \
	NewSourceImages.cs

all: ../$(TARGET)

../$(TARGET): ../../IPlugin.dll ../../Utility.dll $(SRC) $(RESFILES)
	$(MCS) $(SRC) -out:../$(TARGET) -r:IPlugin -r:Mono.Posix -lib:../.. -target:library -pkg:gtk-sharp-2.0 -pkg:glade-sharp-2.0

clean:
	rm -f ../$(TARGET)
