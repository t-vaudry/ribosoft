find_library(MELTING_LIBRARY
        NAMES
        MELTING
        PATH_SUFFIXES
        melting_v4/lib/)

find_path(MELTING_INCLUDE_DIR
        NAMES
        melting.h
        PATH_SUFFIXES
        melting_v4/include/)