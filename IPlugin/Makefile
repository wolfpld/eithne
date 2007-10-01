MCS = mcs

TARGET = ../IPlugin.dll

IPLUGIN = \
	CommSocket.cs \
	Config.cs \
	GConfConfig.cs \
	IBlock.cs \
	ICommImage.cs \
	ICommResult.cs \
	IConfig.cs \
	IFactory.cs \
	IImage.cs \
	IInfo.cs \
	IInPlugin.cs \
	IOutPlugin.cs \
	IPlugin.cs \
	IResult.cs \
	IType.cs \
	PluginException.cs \
	Program.cs \
	RegistryConfig.cs

all: $(TARGET)

$(TARGET): $(IPLUGIN)
	$(MCS) $(IPLUGIN) -out:$(TARGET) -target:library -r:Mono.Posix -unsafe -debug -pkg:gconf-sharp-2.0

clean:
	rm -f $(TARGET) $(TARGET).mdb
