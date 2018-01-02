find_library(RNA_LIBRARY
        NAMES
        RNA
        PATH_SUFFIXES
        viennarna/lib/)

find_path(RNA_INCLUDE_DIR
        NAMES
        ViennaRNA/fold.h
        PATH_SUFFIXES
        viennarna/include/)