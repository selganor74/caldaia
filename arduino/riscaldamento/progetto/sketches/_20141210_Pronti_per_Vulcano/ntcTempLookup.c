/******************************************
 * VETTORE DI CONVERSIONE TEMPERATURA NTC *
 ******************************************/
 int getTempFromAin( int ain ) {
  if (ain > 975) return 111;
  if (ain < 398 ) return 14;
  switch( ain ) {
    case 398: return 15;
    case 399: return 15;
    case 400: return 15;
    case 401: return 15;
    case 402: return 15;
    case 403: return 15;
    case 404: return 15;
    case 405: return 15;
    case 406: return 15;
    case 407: return 15;
    case 408: return 15;
    case 409: return 15;
    case 410: return 16;
    case 411: return 16;
    case 412: return 16;
    case 413: return 16;
    case 414: return 16;
    case 415: return 16;
    case 416: return 16;
    case 417: return 16;
    case 418: return 16;
    case 419: return 16;
    case 420: return 16;
    case 421: return 17;
    case 422: return 17;
    case 423: return 17;
    case 424: return 17;
    case 425: return 17;
    case 426: return 17;
    case 427: return 17;
    case 428: return 17;
    case 429: return 17;
    case 430: return 17;
    case 431: return 17;
    case 432: return 17;
    case 433: return 18;
    case 434: return 18;
    case 435: return 18;
    case 436: return 18;
    case 437: return 18;
    case 438: return 18;
    case 439: return 18;
    case 440: return 18;
    case 441: return 18;
    case 442: return 18;
    case 443: return 18;
    case 444: return 19;
    case 445: return 19;
    case 446: return 19;
    case 447: return 19;
    case 448: return 19;
    case 449: return 19;
    case 450: return 19;
    case 451: return 19;
    case 452: return 19;
    case 453: return 19;
    case 454: return 19;
    case 455: return 20;
    case 456: return 20;
    case 457: return 20;
    case 458: return 20;
    case 459: return 20;
    case 460: return 20;
    case 461: return 20;
    case 462: return 20;
    case 463: return 20;
    case 464: return 20;
    case 465: return 20;
    case 466: return 20;
    case 467: return 21;
    case 468: return 21;
    case 469: return 21;
    case 470: return 21;
    case 471: return 21;
    case 472: return 21;
    case 473: return 21;
    case 474: return 21;
    case 475: return 21;
    case 476: return 21;
    case 477: return 21;
    case 478: return 22;
    case 479: return 22;
    case 480: return 22;
    case 481: return 22;
    case 482: return 22;
    case 483: return 22;
    case 484: return 22;
    case 485: return 22;
    case 486: return 22;
    case 487: return 22;
    case 488: return 22;
    case 489: return 23;
    case 490: return 23;
    case 491: return 23;
    case 492: return 23;
    case 493: return 23;
    case 494: return 23;
    case 495: return 23;
    case 496: return 23;
    case 497: return 23;
    case 498: return 23;
    case 499: return 23;
    case 500: return 23;
    case 501: return 24;
    case 502: return 24;
    case 503: return 24;
    case 504: return 24;
    case 505: return 24;
    case 506: return 24;
    case 507: return 24;
    case 508: return 24;
    case 509: return 24;
    case 510: return 24;
    case 511: return 24;
    case 512: return 25;
    case 513: return 25;
    case 514: return 25;
    case 515: return 25;
    case 516: return 25;
    case 517: return 25;
    case 518: return 25;
    case 519: return 25;
    case 520: return 25;
    case 521: return 25;
    case 522: return 25;
    case 523: return 26;
    case 524: return 26;
    case 525: return 26;
    case 526: return 26;
    case 527: return 26;
    case 528: return 26;
    case 529: return 26;
    case 530: return 26;
    case 531: return 26;
    case 532: return 26;
    case 533: return 26;
    case 534: return 27;
    case 535: return 27;
    case 536: return 27;
    case 537: return 27;
    case 538: return 27;
    case 539: return 27;
    case 540: return 27;
    case 541: return 27;
    case 542: return 27;
    case 543: return 27;
    case 544: return 27;
    case 545: return 28;
    case 546: return 28;
    case 547: return 28;
    case 548: return 28;
    case 549: return 28;
    case 550: return 28;
    case 551: return 28;
    case 552: return 28;
    case 553: return 28;
    case 554: return 28;
    case 555: return 28;
    case 556: return 29;
    case 557: return 29;
    case 558: return 29;
    case 559: return 29;
    case 560: return 29;
    case 561: return 29;
    case 562: return 29;
    case 563: return 29;
    case 564: return 29;
    case 565: return 29;
    case 566: return 29;
    case 567: return 30;
    case 568: return 30;
    case 569: return 30;
    case 570: return 30;
    case 571: return 30;
    case 572: return 30;
    case 573: return 30;
    case 574: return 30;
    case 575: return 30;
    case 576: return 30;
    case 577: return 30;
    case 578: return 31;
    case 579: return 31;
    case 580: return 31;
    case 581: return 31;
    case 582: return 31;
    case 583: return 31;
    case 584: return 31;
    case 585: return 31;
    case 586: return 31;
    case 587: return 31;
    case 588: return 32;
    case 589: return 32;
    case 590: return 32;
    case 591: return 32;
    case 592: return 32;
    case 593: return 32;
    case 594: return 32;
    case 595: return 32;
    case 596: return 32;
    case 597: return 32;
    case 598: return 32;
    case 599: return 33;
    case 600: return 33;
    case 601: return 33;
    case 602: return 33;
    case 603: return 33;
    case 604: return 33;
    case 605: return 33;
    case 606: return 33;
    case 607: return 33;
    case 608: return 33;
    case 609: return 34;
    case 610: return 34;
    case 611: return 34;
    case 612: return 34;
    case 613: return 34;
    case 614: return 34;
    case 615: return 34;
    case 616: return 34;
    case 617: return 34;
    case 618: return 34;
    case 619: return 35;
    case 620: return 35;
    case 621: return 35;
    case 622: return 35;
    case 623: return 35;
    case 624: return 35;
    case 625: return 35;
    case 626: return 35;
    case 627: return 35;
    case 628: return 35;
    case 629: return 36;
    case 630: return 36;
    case 631: return 36;
    case 632: return 36;
    case 633: return 36;
    case 634: return 36;
    case 635: return 36;
    case 636: return 36;
    case 637: return 36;
    case 638: return 36;
    case 639: return 37;
    case 640: return 37;
    case 641: return 37;
    case 642: return 37;
    case 643: return 37;
    case 644: return 37;
    case 645: return 37;
    case 646: return 37;
    case 647: return 37;
    case 648: return 37;
    case 649: return 38;
    case 650: return 38;
    case 651: return 38;
    case 652: return 38;
    case 653: return 38;
    case 654: return 38;
    case 655: return 38;
    case 656: return 38;
    case 657: return 38;
    case 658: return 39;
    case 659: return 39;
    case 660: return 39;
    case 661: return 39;
    case 662: return 39;
    case 663: return 39;
    case 664: return 39;
    case 665: return 39;
    case 666: return 39;
    case 667: return 39;
    case 668: return 40;
    case 669: return 40;
    case 670: return 40;
    case 671: return 40;
    case 672: return 40;
    case 673: return 40;
    case 674: return 40;
    case 675: return 40;
    case 676: return 40;
    case 677: return 41;
    case 678: return 41;
    case 679: return 41;
    case 680: return 41;
    case 681: return 41;
    case 682: return 41;
    case 683: return 41;
    case 684: return 41;
    case 685: return 41;
    case 686: return 42;
    case 687: return 42;
    case 688: return 42;
    case 689: return 42;
    case 690: return 42;
    case 691: return 42;
    case 692: return 42;
    case 693: return 42;
    case 694: return 42;
    case 695: return 43;
    case 696: return 43;
    case 697: return 43;
    case 698: return 43;
    case 699: return 43;
    case 700: return 43;
    case 701: return 43;
    case 702: return 43;
    case 703: return 43;
    case 704: return 44;
    case 705: return 44;
    case 706: return 44;
    case 707: return 44;
    case 708: return 44;
    case 709: return 44;
    case 710: return 44;
    case 711: return 44;
    case 712: return 44;
    case 713: return 45;
    case 714: return 45;
    case 715: return 45;
    case 716: return 45;
    case 717: return 45;
    case 718: return 45;
    case 719: return 45;
    case 720: return 45;
    case 721: return 46;
    case 722: return 46;
    case 723: return 46;
    case 724: return 46;
    case 725: return 46;
    case 726: return 46;
    case 727: return 46;
    case 728: return 46;
    case 729: return 47;
    case 730: return 47;
    case 731: return 47;
    case 732: return 47;
    case 733: return 47;
    case 734: return 47;
    case 735: return 47;
    case 736: return 47;
    case 737: return 48;
    case 738: return 48;
    case 739: return 48;
    case 740: return 48;
    case 741: return 48;
    case 742: return 48;
    case 743: return 48;
    case 744: return 48;
    case 745: return 49;
    case 746: return 49;
    case 747: return 49;
    case 748: return 49;
    case 749: return 49;
    case 750: return 49;
    case 751: return 49;
    case 752: return 49;
    case 753: return 50;
    case 754: return 50;
    case 755: return 50;
    case 756: return 50;
    case 757: return 50;
    case 758: return 50;
    case 759: return 50;
    case 760: return 51;
    case 761: return 51;
    case 762: return 51;
    case 763: return 51;
    case 764: return 51;
    case 765: return 51;
    case 766: return 51;
    case 767: return 52;
    case 768: return 52;
    case 769: return 52;
    case 770: return 52;
    case 771: return 52;
    case 772: return 52;
    case 773: return 52;
    case 774: return 53;
    case 775: return 53;
    case 776: return 53;
    case 777: return 53;
    case 778: return 53;
    case 779: return 53;
    case 780: return 53;
    case 781: return 53;
    case 782: return 54;
    case 783: return 54;
    case 784: return 54;
    case 785: return 54;
    case 786: return 54;
    case 787: return 54;
    case 788: return 54;
    case 789: return 55;
    case 790: return 55;
    case 791: return 55;
    case 792: return 55;
    case 793: return 55;
    case 794: return 55;
    case 795: return 56;
    case 796: return 56;
    case 797: return 56;
    case 798: return 56;
    case 799: return 56;
    case 800: return 56;
    case 801: return 57;
    case 802: return 57;
    case 803: return 57;
    case 804: return 57;
    case 805: return 57;
    case 806: return 57;
    case 807: return 57;
    case 808: return 58;
    case 809: return 58;
    case 810: return 58;
    case 811: return 58;
    case 812: return 58;
    case 813: return 58;
    case 814: return 59;
    case 815: return 59;
    case 816: return 59;
    case 817: return 59;
    case 818: return 59;
    case 819: return 59;
    case 820: return 60;
    case 821: return 60;
    case 822: return 60;
    case 823: return 60;
    case 824: return 60;
    case 825: return 60;
    case 826: return 61;
    case 827: return 61;
    case 828: return 61;
    case 829: return 61;
    case 830: return 61;
    case 831: return 62;
    case 832: return 62;
    case 833: return 62;
    case 834: return 62;
    case 835: return 62;
    case 836: return 62;
    case 837: return 63;
    case 838: return 63;
    case 839: return 63;
    case 840: return 63;
    case 841: return 63;
    case 842: return 64;
    case 843: return 64;
    case 844: return 64;
    case 845: return 64;
    case 846: return 64;
    case 847: return 64;
    case 848: return 65;
    case 849: return 65;
    case 850: return 65;
    case 851: return 65;
    case 852: return 66;
    case 853: return 66;
    case 854: return 66;
    case 855: return 66;
    case 856: return 66;
    case 857: return 67;
    case 858: return 67;
    case 859: return 67;
    case 860: return 67;
    case 861: return 67;
    case 862: return 68;
    case 863: return 68;
    case 864: return 68;
    case 865: return 68;
    case 866: return 68;
    case 867: return 69;
    case 868: return 69;
    case 869: return 69;
    case 870: return 69;
    case 871: return 69;
    case 872: return 70;
    case 873: return 70;
    case 874: return 70;
    case 875: return 70;
    case 876: return 71;
    case 877: return 71;
    case 878: return 71;
    case 879: return 71;
    case 880: return 72;
    case 881: return 72;
    case 882: return 72;
    case 883: return 72;
    case 884: return 73;
    case 885: return 73;
    case 886: return 73;
    case 887: return 73;
    case 888: return 74;
    case 889: return 74;
    case 890: return 74;
    case 891: return 74;
    case 892: return 75;
    case 893: return 75;
    case 894: return 75;
    case 895: return 75;
    case 896: return 76;
    case 897: return 76;
    case 898: return 76;
    case 899: return 77;
    case 900: return 77;
    case 901: return 77;
    case 902: return 77;
    case 903: return 78;
    case 904: return 78;
    case 905: return 78;
    case 906: return 79;
    case 907: return 79;
    case 908: return 79;
    case 909: return 79;
    case 910: return 80;
    case 911: return 80;
    case 912: return 80;
    case 913: return 81;
    case 914: return 81;
    case 915: return 81;
    case 916: return 82;
    case 917: return 82;
    case 918: return 82;
    case 919: return 83;
    case 920: return 83;
    case 921: return 83;
    case 922: return 84;
    case 923: return 84;
    case 924: return 84;
    case 925: return 85;
    case 926: return 85;
    case 927: return 85;
    case 928: return 86;
    case 929: return 86;
    case 930: return 87;
    case 931: return 87;
    case 932: return 87;
    case 933: return 88;
    case 934: return 88;
    case 935: return 88;
    case 936: return 89;
    case 937: return 89;
    case 938: return 90;
    case 939: return 90;
    case 940: return 90;
    case 941: return 91;
    case 942: return 91;
    case 943: return 92;
    case 944: return 92;
    case 945: return 93;
    case 946: return 93;
    case 947: return 94;
    case 948: return 94;
    case 949: return 94;
    case 950: return 95;
    case 951: return 96;
    case 952: return 96;
    case 953: return 97;
    case 954: return 97;
    case 955: return 98;
    case 956: return 98;
    case 957: return 99;
    case 958: return 99;
    case 959: return 100;
    case 960: return 100;
    case 961: return 101;
    case 962: return 102;
    case 963: return 102;
    case 964: return 103;
    case 965: return 103;
    case 966: return 104;
    case 967: return 105;
    case 968: return 105;
    case 969: return 106;
    case 970: return 107;
    case 971: return 107;
    case 972: return 108;
    case 973: return 109;
    case 974: return 109;
    case 975: return 110;
  }
}

