using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Pdf;

public static class IconUnicodeMapper
{
    private static readonly Dictionary<int, char> Icons = new()
    {
        { 0, '\uf042' },
        { 1, '\uf170' },
        { 2, '\uf037' },
        { 3, '\uf039' },
        { 4, '\uf036' },
        { 5, '\uf038' },
        { 6, '\uf270' },
        { 7, '\uf0f9' },
        { 8, '\uf13d' },
        { 9, '\uf17b' },
        { 10, '\uf209' },
        { 11, '\uf103' },
        { 12, '\uf100' },
        { 13, '\uf101' },
        { 14, '\uf102' },
        { 15, '\uf107' },
        { 16, '\uf104' },
        { 17, '\uf105' },
        { 18, '\uf106' },
        { 19, '\uf179' },
        { 20, '\uf187' },
        { 21, '\uf1fe' },
        { 22, '\uf0ab' },
        { 23, '\uf0a8' },
        { 24, '\uf01a' },
        { 25, '\uf190' },
        { 26, '\uf18e' },
        { 27, '\uf01b' },
        { 28, '\uf0a9' },
        { 29, '\uf0aa' },
        { 30, '\uf063' },
        { 31, '\uf060' },
        { 32, '\uf061' },
        { 33, '\uf062' },
        { 34, '\uf047' },
        { 35, '\uf0b2' },
        { 36, '\uf07e' },
        { 37, '\uf07d' },
        { 38, '\uf069' },
        { 39, '\uf1fa' },
        { 40, '\uf1b9' },
        { 41, '\uf04a' },
        { 42, '\uf24e' },
        { 43, '\uf05e' },
        { 44, '\uf19c' },
        { 45, '\uf080' },
        { 46, '\uf080' },
        { 47, '\uf02a' },
        { 48, '\uf0c9' },
        { 49, '\uf244' },
        { 50, '\uf243' },
        { 51, '\uf242' },
        { 52, '\uf241' },
        { 53, '\uf240' },
        { 54, '\uf244' },
        { 55, '\uf240' },
        { 56, '\uf242' },
        { 57, '\uf243' },
        { 58, '\uf241' },
        { 59, '\uf236' },
        { 60, '\uf0fc' },
        { 61, '\uf1b4' },
        { 62, '\uf1b5' },
        { 63, '\uf0f3' },
        { 64, '\uf0a2' },
        { 65, '\uf1f6' },
        { 66, '\uf1f7' },
        { 67, '\uf206' },
        { 68, '\uf1e5' },
        { 69, '\uf1fd' },
        { 70, '\uf171' },
        { 71, '\uf172' },
        { 72, '\uf15a' },
        { 73, '\uf27e' },
        { 74, '\uf293' },
        { 75, '\uf294' },
        { 76, '\uf032' },
        { 77, '\uf0e7' },
        { 78, '\uf1e2' },
        { 79, '\uf02d' },
        { 80, '\uf02e' },
        { 81, '\uf097' },
        { 82, '\uf0b1' },
        { 83, '\uf15a' },
        { 84, '\uf188' },
        { 85, '\uf1ad' },
        { 86, '\uf0f7' },
        { 87, '\uf0a1' },
        { 88, '\uf140' },
        { 89, '\uf207' },
        { 90, '\uf20d' },
        { 91, '\uf1ba' },
        { 92, '\uf1ec' },
        { 93, '\uf073' },
        { 94, '\uf274' },
        { 95, '\uf272' },
        { 96, '\uf133' },
        { 97, '\uf271' },
        { 98, '\uf273' },
        { 99, '\uf030' },
        { 100, '\uf083' },
        { 101, '\uf1b9' },
        { 102, '\uf0d7' },
        { 103, '\uf0d9' },
        { 104, '\uf0da' },
        { 105, '\uf150' },
        { 106, '\uf191' },
        { 107, '\uf152' },
        { 108, '\uf151' },
        { 109, '\uf0d8' },
        { 110, '\uf218' },
        { 111, '\uf217' },
        { 112, '\uf20a' },
        { 113, '\uf1f3' },
        { 114, '\uf24c' },
        { 115, '\uf1f2' },
        { 116, '\uf24b' },
        { 117, '\uf1f1' },
        { 118, '\uf1f4' },
        { 119, '\uf1f5' },
        { 120, '\uf1f0' },
        { 121, '\uf0a3' },
        { 122, '\uf0c1' },
        { 123, '\uf127' },
        { 124, '\uf00c' },
        { 125, '\uf058' },
        { 126, '\uf05d' },
        { 127, '\uf14a' },
        { 128, '\uf046' },
        { 129, '\uf13a' },
        { 130, '\uf137' },
        { 131, '\uf138' },
        { 132, '\uf139' },
        { 133, '\uf078' },
        { 134, '\uf053' },
        { 135, '\uf054' },
        { 136, '\uf077' },
        { 137, '\uf1ae' },
        { 138, '\uf268' },
        { 139, '\uf111' },
        { 140, '\uf10c' },
        { 141, '\uf1ce' },
        { 142, '\uf1db' },
        { 143, '\uf0ea' },
        { 144, '\uf017' },
        { 145, '\uf24d' },
        { 146, '\uf00d' },
        { 147, '\uf0c2' },
        { 148, '\uf0ed' },
        { 149, '\uf0ee' },
        { 150, '\uf157' },
        { 151, '\uf121' },
        { 152, '\uf126' },
        { 153, '\uf1cb' },
        { 154, '\uf284' },
        { 155, '\uf0f4' },
        { 156, '\uf013' },
        { 157, '\uf085' },
        { 158, '\uf0db' },
        { 159, '\uf075' },
        { 160, '\uf0e5' },
        { 161, '\uf27a' },
        { 162, '\uf27b' },
        { 163, '\uf086' },
        { 164, '\uf0e6' },
        { 165, '\uf14e' },
        { 166, '\uf066' },
        { 167, '\uf20e' },
        { 168, '\uf26d' },
        { 169, '\uf0c5' },
        { 170, '\uf1f9' },
        { 171, '\uf25e' },
        { 172, '\uf09d' },
        { 173, '\uf283' },
        { 174, '\uf125' },
        { 175, '\uf05b' },
        { 176, '\uf13c' },
        { 177, '\uf1b2' },
        { 178, '\uf1b3' },
        { 179, '\uf0c4' },
        { 180, '\uf0f5' },
        { 181, '\uf0e4' },
        { 182, '\uf210' },
        { 183, '\uf1c0' },
        { 184, '\uf03b' },
        { 185, '\uf1a5' },
        { 186, '\uf108' },
        { 187, '\uf1bd' },
        { 188, '\uf219' },
        { 189, '\uf1a6' },
        { 190, '\uf155' },
        { 191, '\uf192' },
        { 192, '\uf019' },
        { 193, '\uf17d' },
        { 194, '\uf16b' },
        { 195, '\uf1a9' },
        { 196, '\uf282' },
        { 197, '\uf044' },
        { 198, '\uf052' },
        { 199, '\uf141' },
        { 200, '\uf142' },
        { 201, '\uf1d1' },
        { 202, '\uf0e0' },
        { 203, '\uf003' },
        { 204, '\uf199' },
        { 205, '\uf12d' },
        { 206, '\uf153' },
        { 207, '\uf153' },
        { 208, '\uf0ec' },
        { 209, '\uf12a' },
        { 210, '\uf06a' },
        { 211, '\uf071' },
        { 212, '\uf065' },
        { 213, '\uf23e' },
        { 214, '\uf08e' },
        { 215, '\uf14c' },
        { 216, '\uf06e' },
        { 217, '\uf070' },
        { 218, '\uf1fb' },
        { 219, '\uf09a' },
        { 220, '\uf09a' },
        { 221, '\uf230' },
        { 222, '\uf082' },
        { 223, '\uf049' },
        { 224, '\uf050' },
        { 225, '\uf1ac' },
        { 226, '\uf09e' },
        { 227, '\uf182' },
        { 228, '\uf0fb' },
        { 229, '\uf15b' },
        { 230, '\uf1c6' },
        { 231, '\uf1c7' },
        { 232, '\uf1c9' },
        { 233, '\uf1c3' },
        { 234, '\uf1c5' },
        { 235, '\uf1c8' },
        { 236, '\uf016' },
        { 237, '\uf1c1' },
        { 238, '\uf1c5' },
        { 239, '\uf1c5' },
        { 240, '\uf1c4' },
        { 241, '\uf1c7' },
        { 242, '\uf15c' },
        { 243, '\uf0f6' },
        { 244, '\uf1c8' },
        { 245, '\uf1c2' },
        { 246, '\uf1c6' },
        { 247, '\uf0c5' },
        { 248, '\uf008' },
        { 249, '\uf0b0' },
        { 250, '\uf06d' },
        { 251, '\uf134' },
        { 252, '\uf269' },
        { 253, '\uf024' },
        { 254, '\uf11e' },
        { 255, '\uf11d' },
        { 256, '\uf0e7' },
        { 257, '\uf0c3' },
        { 258, '\uf16e' },
        { 259, '\uf0c7' },
        { 260, '\uf07b' },
        { 261, '\uf114' },
        { 262, '\uf07c' },
        { 263, '\uf115' },
        { 264, '\uf031' },
        { 265, '\uf280' },
        { 266, '\uf286' },
        { 267, '\uf211' },
        { 268, '\uf04e' },
        { 269, '\uf180' },
        { 270, '\uf119' },
        { 271, '\uf1e3' },
        { 272, '\uf11b' },
        { 273, '\uf0e3' },
        { 274, '\uf154' },
        { 275, '\uf1d1' },
        { 276, '\uf013' },
        { 277, '\uf085' },
        { 278, '\uf22d' },
        { 279, '\uf265' },
        { 280, '\uf260' },
        { 281, '\uf261' },
        { 282, '\uf06b' },
        { 283, '\uf1d3' },
        { 284, '\uf1d2' },
        { 285, '\uf09b' },
        { 286, '\uf113' },
        { 287, '\uf092' },
        { 288, '\uf184' },
        { 289, '\uf000' },
        { 290, '\uf0ac' },
        { 291, '\uf1a0' },
        { 292, '\uf0d5' },
        { 293, '\uf0d4' },
        { 294, '\uf1ee' },
        { 295, '\uf19d' },
        { 296, '\uf184' },
        { 297, '\uf0c0' },
        { 298, '\uf0fd' },
        { 299, '\uf1d4' },
        { 300, '\uf255' },
        { 301, '\uf258' },
        { 302, '\uf0a7' },
        { 303, '\uf0a5' },
        { 304, '\uf0a4' },
        { 305, '\uf0a6' },
        { 306, '\uf256' },
        { 307, '\uf25b' },
        { 308, '\uf25a' },
        { 309, '\uf255' },
        { 310, '\uf257' },
        { 311, '\uf259' },
        { 312, '\uf256' },
        { 313, '\uf292' },
        { 314, '\uf0a0' },
        { 315, '\uf1dc' },
        { 316, '\uf025' },
        { 317, '\uf004' },
        { 318, '\uf08a' },
        { 319, '\uf21e' },
        { 320, '\uf1da' },
        { 321, '\uf015' },
        { 322, '\uf0f8' },
        { 323, '\uf236' },
        { 324, '\uf254' },
        { 325, '\uf251' },
        { 326, '\uf252' },
        { 327, '\uf253' },
        { 328, '\uf253' },
        { 329, '\uf252' },
        { 330, '\uf250' },
        { 331, '\uf251' },
        { 332, '\uf27c' },
        { 333, '\uf13b' },
        { 334, '\uf246' },
        { 335, '\uf20b' },
        { 336, '\uf03e' },
        { 337, '\uf01c' },
        { 338, '\uf03c' },
        { 339, '\uf275' },
        { 340, '\uf129' },
        { 341, '\uf05a' },
        { 342, '\uf156' },
        { 343, '\uf16d' },
        { 344, '\uf19c' },
        { 345, '\uf26b' },
        { 346, '\uf224' },
        { 347, '\uf208' },
        { 348, '\uf033' },
        { 349, '\uf1aa' },
        { 350, '\uf157' },
        { 351, '\uf1cc' },
        { 352, '\uf084' },
        { 353, '\uf11c' },
        { 354, '\uf159' },
        { 355, '\uf1ab' },
        { 356, '\uf109' },
        { 357, '\uf202' },
        { 358, '\uf203' },
        { 359, '\uf06c' },
        { 360, '\uf212' },
        { 361, '\uf0e3' },
        { 362, '\uf094' },
        { 363, '\uf149' },
        { 364, '\uf148' },
        { 365, '\uf1cd' },
        { 366, '\uf1cd' },
        { 367, '\uf1cd' },
        { 368, '\uf1cd' },
        { 369, '\uf0eb' },
        { 370, '\uf201' },
        { 371, '\uf0c1' },
        { 372, '\uf0e1' },
        { 373, '\uf08c' },
        { 374, '\uf17c' },
        { 375, '\uf03a' },
        { 376, '\uf022' },
        { 377, '\uf0cb' },
        { 378, '\uf0ca' },
        { 379, '\uf124' },
        { 380, '\uf023' },
        { 381, '\uf175' },
        { 382, '\uf177' },
        { 383, '\uf178' },
        { 384, '\uf176' },
        { 385, '\uf0d0' },
        { 386, '\uf076' },
        { 387, '\uf064' },
        { 388, '\uf112' },
        { 389, '\uf122' },
        { 390, '\uf183' },
        { 391, '\uf279' },
        { 392, '\uf041' },
        { 393, '\uf278' },
        { 394, '\uf276' },
        { 395, '\uf277' },
        { 396, '\uf222' },
        { 397, '\uf227' },
        { 398, '\uf229' },
        { 399, '\uf22b' },
        { 400, '\uf22a' },
        { 401, '\uf136' },
        { 402, '\uf20c' },
        { 403, '\uf23a' },
        { 404, '\uf0fa' },
        { 405, '\uf11a' },
        { 406, '\uf223' },
        { 407, '\uf130' },
        { 408, '\uf131' },
        { 409, '\uf068' },
        { 410, '\uf056' },
        { 411, '\uf146' },
        { 412, '\uf147' },
        { 413, '\uf289' },
        { 414, '\uf10b' },
        { 415, '\uf10b' },
        { 416, '\uf285' },
        { 417, '\uf0d6' },
        { 418, '\uf186' },
        { 419, '\uf19d' },
        { 420, '\uf21c' },
        { 421, '\uf245' },
        { 422, '\uf001' },
        { 423, '\uf0c9' },
        { 424, '\uf22c' },
        { 425, '\uf1ea' },
        { 426, '\uf247' },
        { 427, '\uf248' },
        { 428, '\uf263' },
        { 429, '\uf264' },
        { 430, '\uf23d' },
        { 431, '\uf19b' },
        { 432, '\uf26a' },
        { 433, '\uf23c' },
        { 434, '\uf03b' },
        { 435, '\uf18c' },
        { 436, '\uf1fc' },
        { 437, '\uf1d8' },
        { 438, '\uf1d9' },
        { 439, '\uf0c6' },
        { 440, '\uf1dd' },
        { 441, '\uf0ea' },
        { 442, '\uf04c' },
        { 443, '\uf28b' },
        { 444, '\uf28c' },
        { 445, '\uf1b0' },
        { 446, '\uf1ed' },
        { 447, '\uf040' },
        { 448, '\uf14b' },
        { 449, '\uf044' },
        { 450, '\uf295' },
        { 451, '\uf095' },
        { 452, '\uf098' },
        { 453, '\uf03e' },
        { 454, '\uf03e' },
        { 455, '\uf200' },
        { 456, '\uf2ae' },
        { 457, '\uf1a8' },
        { 458, '\uf0d2' },
        { 459, '\uf231' },
        { 460, '\uf0d3' },
        { 461, '\uf072' },
        { 462, '\uf04b' },
        { 463, '\uf144' },
        { 464, '\uf01d' },
        { 465, '\uf1e6' },
        { 466, '\uf067' },
        { 467, '\uf055' },
        { 468, '\uf0fe' },
        { 469, '\uf196' },
        { 470, '\uf011' },
        { 471, '\uf02f' },
        { 472, '\uf288' },
        { 473, '\uf12e' },
        { 474, '\uf1d6' },
        { 475, '\uf029' },
        { 476, '\uf128' },
        { 477, '\uf059' },
        { 478, '\uf10d' },
        { 479, '\uf10e' },
        { 480, '\uf1d0' },
        { 481, '\uf074' },
        { 482, '\uf1d0' },
        { 483, '\uf1b8' },
        { 484, '\uf1a1' },
        { 485, '\uf281' },
        { 486, '\uf1a2' },
        { 487, '\uf021' },
        { 488, '\uf25d' },
        { 489, '\uf00d' },
        { 490, '\uf18b' },
        { 491, '\uf0c9' },
        { 492, '\uf01e' },
        { 493, '\uf112' },
        { 494, '\uf122' },
        { 495, '\uf079' },
        { 496, '\uf157' },
        { 497, '\uf018' },
        { 498, '\uf135' },
        { 499, '\uf0e2' },
        { 500, '\uf01e' },
        { 501, '\uf158' },
        { 502, '\uf09e' },
        { 503, '\uf143' },
        { 504, '\uf158' },
        { 505, '\uf158' },
        { 506, '\uf156' },
        { 507, '\uf267' },
        { 508, '\uf0c7' },
        { 509, '\uf0c4' },
        { 510, '\uf28a' },
        { 511, '\uf002' },
        { 512, '\uf010' },
        { 513, '\uf00e' },
        { 514, '\uf213' },
        { 515, '\uf1d8' },
        { 516, '\uf1d9' },
        { 517, '\uf233' },
        { 518, '\uf064' },
        { 519, '\uf1e0' },
        { 520, '\uf1e1' },
        { 521, '\uf14d' },
        { 522, '\uf045' },
        { 523, '\uf20b' },
        { 524, '\uf20b' },
        { 525, '\uf132' },
        { 526, '\uf21a' },
        { 527, '\uf214' },
        { 528, '\uf290' },
        { 529, '\uf291' },
        { 530, '\uf07a' },
        { 531, '\uf090' },
        { 532, '\uf08b' },
        { 533, '\uf012' },
        { 534, '\uf215' },
        { 535, '\uf0e8' },
        { 536, '\uf216' },
        { 537, '\uf17e' },
        { 538, '\uf198' },
        { 539, '\uf1de' },
        { 540, '\uf1e7' },
        { 541, '\uf118' },
        { 542, '\uf1e3' },
        { 543, '\uf0dc' },
        { 544, '\uf15d' },
        { 545, '\uf15e' },
        { 546, '\uf160' },
        { 547, '\uf161' },
        { 548, '\uf0de' },
        { 549, '\uf0dd' },
        { 550, '\uf0dd' },
        { 551, '\uf162' },
        { 552, '\uf163' },
        { 553, '\uf0de' },
        { 554, '\uf1be' },
        { 555, '\uf197' },
        { 556, '\uf110' },
        { 557, '\uf1b1' },
        { 558, '\uf1bc' },
        { 559, '\uf0c8' },
        { 560, '\uf096' },
        { 561, '\uf18d' },
        { 562, '\uf16c' },
        { 563, '\uf005' },
        { 564, '\uf089' },
        { 565, '\uf123' },
        { 566, '\uf123' },
        { 567, '\uf123' },
        { 568, '\uf006' },
        { 569, '\uf1b6' },
        { 570, '\uf1b7' },
        { 571, '\uf048' },
        { 572, '\uf051' },
        { 573, '\uf0f1' },
        { 574, '\uf249' },
        { 575, '\uf24a' },
        { 576, '\uf04d' },
        { 577, '\uf28d' },
        { 578, '\uf28e' },
        { 579, '\uf21d' },
        { 580, '\uf0cc' },
        { 581, '\uf1a4' },
        { 582, '\uf1a3' },
        { 583, '\uf12c' },
        { 584, '\uf239' },
        { 585, '\uf0f2' },
        { 586, '\uf185' },
        { 587, '\uf12b' },
        { 588, '\uf1cd' },
        { 589, '\uf0ce' },
        { 590, '\uf10a' },
        { 591, '\uf0e4' },
        { 592, '\uf02b' },
        { 593, '\uf02c' },
        { 594, '\uf0ae' },
        { 595, '\uf1ba' },
        { 596, '\uf26c' },
        { 597, '\uf1d5' },
        { 598, '\uf120' },
        { 599, '\uf034' },
        { 600, '\uf035' },
        { 601, '\uf00a' },
        { 602, '\uf009' },
        { 603, '\uf00b' },
        { 604, '\uf08d' },
        { 605, '\uf165' },
        { 606, '\uf088' },
        { 607, '\uf087' },
        { 608, '\uf164' },
        { 609, '\uf145' },
        { 610, '\uf00d' },
        { 611, '\uf057' },
        { 612, '\uf05c' },
        { 613, '\uf043' },
        { 614, '\uf150' },
        { 615, '\uf191' },
        { 616, '\uf204' },
        { 617, '\uf205' },
        { 618, '\uf152' },
        { 619, '\uf151' },
        { 620, '\uf25c' },
        { 621, '\uf238' },
        { 622, '\uf224' },
        { 623, '\uf225' },
        { 624, '\uf1f8' },
        { 625, '\uf014' },
        { 626, '\uf1bb' },
        { 627, '\uf181' },
        { 628, '\uf262' },
        { 629, '\uf091' },
        { 630, '\uf0d1' },
        { 631, '\uf195' },
        { 632, '\uf1e4' },
        { 633, '\uf173' },
        { 634, '\uf174' },
        { 635, '\uf195' },
        { 636, '\uf26c' },
        { 637, '\uf1e8' },
        { 638, '\uf099' },
        { 639, '\uf081' },
        { 640, '\uf0e9' },
        { 641, '\uf0cd' },
        { 642, '\uf0e2' },
        { 643, '\uf19c' },
        { 644, '\uf127' },
        { 645, '\uf09c' },
        { 646, '\uf13e' },
        { 647, '\uf0dc' },
        { 648, '\uf093' },
        { 649, '\uf287' },
        { 650, '\uf155' },
        { 651, '\uf007' },
        { 652, '\uf0f0' },
        { 653, '\uf234' },
        { 654, '\uf21b' },
        { 655, '\uf235' },
        { 656, '\uf0c0' },
        { 657, '\uf221' },
        { 658, '\uf226' },
        { 659, '\uf228' },
        { 660, '\uf237' },
        { 661, '\uf03d' },
        { 662, '\uf27d' },
        { 663, '\uf194' },
        { 664, '\uf1ca' },
        { 665, '\uf189' },
        { 666, '\uf027' },
        { 667, '\uf026' },
        { 668, '\uf028' },
        { 669, '\uf071' },
        { 670, '\uf1d7' },
        { 671, '\uf18a' },
        { 672, '\uf1d7' },
        { 673, '\uf232' },
        { 674, '\uf193' },
        { 675, '\uf1eb' },
        { 676, '\uf266' },
        { 677, '\uf17a' },
        { 678, '\uf159' },
        { 679, '\uf19a' },
        { 680, '\uf0ad' },
        { 681, '\uf168' },
        { 682, '\uf169' },
        { 683, '\uf23b' },
        { 684, '\uf1d4' },
        { 685, '\uf19e' },
        { 686, '\uf23b' },
        { 687, '\uf1d4' },
        { 688, '\uf1e9' },
        { 689, '\uf157' },
        { 690, '\uf167' },
        { 691, '\uf16a' },
        { 692, '\uf166' }
    };

    public static char GetUnicode(this IconType icon) => Icons[(int)icon];
}