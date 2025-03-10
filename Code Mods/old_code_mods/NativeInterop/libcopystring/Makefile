LIBRARY_NAME = libcopystring.so
SOURCE_FILE = copystring.c

all: $(LIBRARY_NAME)

$(LIBRARY_NAME): $(SOURCE_FILE)
	gcc -shared -o $(LIBRARY_NAME) $(SOURCE_FILE)

clean:
	rm -f $(LIBRARY_NAME)