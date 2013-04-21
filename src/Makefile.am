
CSC = gmcs -debug

TARGET_NAME = tomboy-reminder
TARGET = $(TARGET_NAME).dll

CSFLAGS = -target:library

CSFILES =   				\
	$(srcdir)/Reminder.cs  \
	$(srcdir)/MagicTime.cs \
	$(srcdir)/MagicDate.cs \
	$(srcdir)/MagicParser.cs \
	$(srcdir)/MagicUtils.cs 

RESOURCES = \
	-resource:$(srcdir)/Reminder.addin.xml

ASSEMBLIES = -pkg:tomboy-addins

$(TARGET): $(CSFILES)
	$(CSC) -out:$@ $(CSFLAGS) $(ASSEMBLIES) $(RESOURCES) $^

all: $(TARGET)

install-data-local: $(TARGET)
	$(mkinstalldirs) $(DESTDIR)$(TOMBOY_ADDINSDIR)
	$(INSTALL_DATA) $(TARGET) $(DESTDIR)$(TOMBOY_ADDINSDIR)

uninstall-local:
	rm -f $(TOMBOY_ADDINSDIR)/$(TARGET)


EXTRA_DIST =			\
	$(CSFILES)		\
	Reminder.addin.xml


CLEANFILES =			\
	$(TARGET)		\
	$(TARGET).mdb
