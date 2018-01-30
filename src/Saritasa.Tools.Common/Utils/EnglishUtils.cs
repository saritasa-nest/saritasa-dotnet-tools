// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// English utilities.
    /// </summary>
    /// <remarks>
    /// Used sources:
    /// https://github.com/itmasterspro/PluralizeService.Core/blob/master/PluralizeService.Core/English/Adapters/EnglishMetaDataAdapter.cs
    /// https://github.com/Microsoft/referencesource/blob/master/System.Data.Entity.Design/System/Data/Entity/Design/PluralizationService/EnglishPluralizationService.cs
    /// </remarks>
    public static class EnglishUtils
    {
        private static readonly string[] knownPluralWords;

        private static readonly string[] uninflectiveSuffixList = { "fish", "ois", "sheep", "deer", "pos", "itis", "ism" };

        private static readonly string[] uninflectiveWordList =
        {
            "bison",
            "flounder",
            "pliers",
            "bream",
            "gallows",
            "proceedings",
            "breeches",
            "graffiti",
            "rabies",
            "britches",
            "headquarters",
            "salmon",
            "carp",
            "herpes",
            "scissors",
            "chassis",
            "high-jinks",
            "sea-bass",
            "clippers",
            "homework",
            "series",
            "cod",
            "innings",
            "shears",
            "contretemps",
            "jackanapes",
            "species",
            "corps",
            "mackerel",
            "swine",
            "debris",
            "measles",
            "trout",
            "diabetes",
            "mews",
            "tuna",
            "djinn",
            "mumps",
            "whiting",
            "eland",
            "news",
            "wildebeest",
            "elk",
            "pincers",
            "police",
            "hair",
            "ice",
            "chaos",
            "milk",
            "cotton",
            "pneumonoultramicroscopicsilicovolcanoconiosis",
            "information",
            "aircraft",
            "scabies",
            "traffic",
            "corn",
            "millet",
            "rice",
            "hay",
            "hemp",
            "tobacco",
            "cabbage",
            "okra",
            "broccoli",
            "asparagus",
            "lettuce",
            "beef",
            "pork",
            "venison",
            "mutton",
            "cattle",
            "offspring",
            "molasses",
            "shambles",
            "shingles"
        };

        private static readonly IDictionary<string, string> irregularVerbList = new Dictionary<string, string>
        {
            { "am", "are" },
            { "are", "are" },
            { "is", "are" },
            { "was", "were" },
            { "were", "were" },
            { "has", "have" },
            { "have", "have" }
        };

        private static readonly string[] pronounList =
        {
            "I",
            "we",
            "you",
            "he",
            "she",
            "they",
            "it",
            "me",
            "us",
            "him",
            "her",
            "them",
            "myself",
            "ourselves",
            "yourself",
            "himself",
            "herself",
            "itself",
            "oneself",
            "oneselves",
            "my",
            "our",
            "your",
            "his",
            "their",
            "its",
            "mine",
            "yours",
            "hers",
            "theirs",
            "this",
            "that",
            "these",
            "those",
            "all",
            "another",
            "any",
            "anybody",
            "anyone",
            "anything",
            "both",
            "each",
            "other",
            "either",
            "everyone",
            "everybody",
            "everything",
            "most",
            "much",
            "nothing",
            "nobody",
            "none",
            "one",
            "others",
            "some",
            "somebody",
            "someone",
            "something",
            "what",
            "whatever",
            "which",
            "whichever",
            "who",
            "whoever",
            "whom",
            "whomever",
            "whose"
        };

        private static readonly IDictionary<string, string> irregularPluralsDictionary = new Dictionary<string, string>
        {
            { "brother", "brothers" },
            { "child", "children" },
            { "cow", "cows" },
            { "ephemeris", "ephemerides" },
            { "genie", "genies" },
            { "money", "moneys" },
            { "mongoose", "mongooses" },
            { "mythos", "mythoi" },
            { "octopus", "octopuses" },
            { "ox", "oxen" },
            { "soliloquy", "soliloquies" },
            { "trilby", "trilbys" },
            { "crisis", "crises" },
            { "synopsis", "synopses" },
            { "rose", "roses" },
            { "gas", "gases" },
            { "bus", "buses" },
            { "axis", "axes" },
            { "memo", "memos" },
            { "casino", "casinos" },
            { "silo", "silos" },
            { "stereo", "stereos" },
            { "studio", "studios" },
            { "lens", "lenses" },
            { "alias", "aliases" },
            { "pie", "pies" },
            { "corpus", "corpora" },
            { "viscus", "viscera" },
            { "hippopotamus", "hippopotami" },
            { "trace", "traces" },
            { "person", "people" },
            { "chili", "chilies" },
            { "analysis", "analyses" },
            { "basis", "bases" },
            { "neurosis", "neuroses" },
            { "oasis", "oases" },
            { "synthesis", "syntheses" },
            { "thesis", "theses" },
            { "change", "changes" },
            { "lie", "lies" },
            { "calorie", "calories" },
            { "freebie", "freebies" },
            { "case", "cases" },
            { "house", "houses" },
            { "valve", "valves" },
            { "cloth", "clothes" },
            { "tie", "ties" },
            { "movie", "movies" },
            { "bonus", "bonuses" },
            { "specimen", "specimens" }
        };

        private static readonly IDictionary<string, string> assimilatedClassicalInflectionDictionary = new Dictionary<string, string>()
        {
            { "alumna", "alumnae" },
            { "alga", "algae" },
            { "vertebra", "vertebrae" },
            { "codex", "codices" },
            { "murex", "murices" },
            { "silex", "silices" },
            { "aphelion", "aphelia" },
            { "hyperbaton", "hyperbata" },
            { "perihelion", "perihelia" },
            { "asyndeton", "asyndeta" },
            { "noumenon", "noumena" },
            { "phenomenon", "phenomena" },
            { "criterion", "criteria" },
            { "organon", "organa" },
            { "prolegomenon", "prolegomena" },
            { "agendum", "agenda" },
            { "datum", "data" },
            { "extremum", "extrema" },
            { "bacterium", "bacteria" },
            { "desideratum", "desiderata" },
            { "stratum", "strata" },
            { "candelabrum", "candelabra" },
            { "erratum", "errata" },
            { "ovum", "ova" },
            { "forum", "fora" },
            { "addendum", "addenda" },
            { "stadium", "stadia" },
            { "automaton", "automata" },
            { "polyhedron", "polyhedra" }
        };

        private static readonly IDictionary<string, string> oSuffixDictionary = new Dictionary<string, string>()
        {
            { "albino", "albinos" },
            { "generalissimo", "generalissimos" },
            { "manifesto", "manifestos" },
            { "archipelago", "archipelagos" },
            { "ghetto", "ghettos" },
            { "medico", "medicos" },
            { "armadillo", "armadillos" },
            { "guano", "guanos" },
            { "octavo", "octavos" },
            { "commando", "commandos" },
            { "inferno", "infernos" },
            { "photo", "photos" },
            { "ditto", "dittos" },
            { "jumbo", "jumbos" },
            { "pro", "pros" },
            { "dynamo", "dynamos" },
            { "lingo", "lingos" },
            { "quarto", "quartos" },
            { "embryo", "embryos" },
            { "lumbago", "lumbagos" },
            { "rhino", "rhinos" },
            { "fiasco", "fiascos" },
            { "magneto", "magnetos" },
            { "stylo", "stylos" }
        };

        private static readonly IDictionary<string, string> classicalInflectionDictionary = new Dictionary<string, string>
        {
            { "stamen", "stamina" },
            { "foramen", "foramina" },
            { "lumen", "lumina" },
            { "anathema", "anathemata" },
            { "enema", "enemata" },
            { "oedema", "oedemata" },
            { "bema", "bemata" },
            { "enigma", "enigmata" },
            { "sarcoma", "sarcomata" },
            { "carcinoma", "carcinomata" },
            { "gumma", "gummata" },
            { "schema", "schemata" },
            { "charisma", "charismata" },
            { "lemma", "lemmata" },
            { "soma", "somata" },
            { "diploma", "diplomata" },
            { "lymphoma", "lymphomata" },
            { "stigma", "stigmata" },
            { "dogma", "dogmata" },
            { "magma", "magmata" },
            { "stoma", "stomata" },
            { "drama", "dramata" },
            { "melisma", "melismata" },
            { "trauma", "traumata" },
            { "edema", "edemata" },
            { "miasma", "miasmata" },
            { "abscissa", "abscissae" },
            { "formula", "formulae" },
            { "medusa", "medusae" },
            { "amoeba", "amoebae" },
            { "hydra", "hydrae" },
            { "nebula", "nebulae" },
            { "antenna", "antennae" },
            { "hyperbola", "hyperbolae" },
            { "nova", "novae" },
            { "aurora", "aurorae" },
            { "lacuna", "lacunae" },
            { "parabola", "parabolae" },
            { "apex", "apices" },
            { "latex", "latices" },
            { "vertex", "vertices" },
            { "cortex", "cortices" },
            { "pontifex", "pontifices" },
            { "vortex", "vortices" },
            { "index", "indices" },
            { "simplex", "simplices" },
            { "iris", "irides" },
            { "clitoris", "clitorides" },
            { "alto", "alti" },
            { "contralto", "contralti" },
            { "soprano", "soprani" },
            { "basso", "bassi" },
            { "crescendo", "crescendi" },
            { "tempo", "tempi" },
            { "canto", "canti" },
            { "solo", "soli" },
            { "aquarium", "aquaria" },
            { "interregnum", "interregna" },
            { "quantum", "quanta" },
            { "compendium", "compendia" },
            { "lustrum", "lustra" },
            { "rostrum", "rostra" },
            { "consortium", "consortia" },
            { "maximum", "maxima" },
            { "spectrum", "spectra" },
            { "cranium", "crania" },
            { "medium", "media" },
            { "speculum", "specula" },
            { "curriculum", "curricula" },
            { "memorandum", "memoranda" },
            { "stadium", "stadia" },
            { "dictum", "dicta" },
            { "millenium", "millenia" },
            { "trapezium", "trapezia" },
            { "emporium", "emporia" },
            { "minimum", "minima" },
            { "ultimatum", "ultimata" },
            { "enconium", "enconia" },
            { "momentum", "momenta" },
            { "vacuum", "vacua" },
            { "gymnasium", "gymnasia" },
            { "optimum", "optima" },
            { "velum", "vela" },
            { "honorarium", "honoraria" },
            { "phylum", "phyla" },
            { "focus", "foci" },
            { "nimbus", "nimbi" },
            { "succubus", "succubi" },
            { "fungus", "fungi" },
            { "nucleolus", "nucleoli" },
            { "torus", "tori" },
            { "genius", "genii" },
            { "radius", "radii" },
            { "umbilicus", "umbilici" },
            { "incubus", "incubi" },
            { "stylus", "styli" },
            { "uterus", "uteri" },
            { "stimulus", "stimuli" },
            { "apparatus", "apparatus" },
            { "impetus", "impetus" },
            { "prospectus", "prospectus" },
            { "cantus", "cantus" },
            { "nexus", "nexus" },
            { "sinus", "sinus" },
            { "coitus", "coitus" },
            { "plexus", "plexus" },
            { "status", "status" },
            { "hiatus", "hiatus" },
            { "afreet", "afreeti" },
            { "afrit", "afriti" },
            { "efreet", "efreeti" },
            { "cherub", "cherubim" },
            { "goy", "goyim" },
            { "seraph", "seraphim" },
            { "alumnus", "alumni" }
        };

        private static readonly IDictionary<string, string> wordsEndingWithSeDictionary = new Dictionary<string, string>
        {
            { "house", "houses" },
            { "case", "cases" },
            { "enterprise", "enterprises" },
            { "purchase", "purchases" },
            { "surprise", "surprises" },
            { "release", "releases" },
            { "disease", "diseases" },
            { "promise", "promises" },
            { "refuse", "refuses" },
            { "whose", "whoses" },
            { "phase", "phases" },
            { "noise", "noises" },
            { "nurse", "nurses" },
            { "rose", "roses" },
            { "franchise", "franchises" },
            { "supervise", "supervises" },
            { "farmhouse", "farmhouses" },
            { "suitcase", "suitcases" },
            { "recourse", "recourses" },
            { "impulse", "impulses" },
            { "license", "licenses" },
            { "diocese", "dioceses" },
            { "excise", "excises" },
            { "demise", "demises" },
            { "blouse", "blouses" },
            { "bruise", "bruises" },
            { "misuse", "misuses" },
            { "curse", "curses" },
            { "prose", "proses" },
            { "purse", "purses" },
            { "goose", "gooses" },
            { "tease", "teases" },
            { "poise", "poises" },
            { "vase", "vases" },
            { "fuse", "fuses" },
            { "muse", "muses" },
            { "slaughterhouse", "slaughterhouses" },
            { "clearinghouse", "clearinghouses" },
            { "endonuclease", "endonucleases" },
            { "steeplechase", "steeplechases" },
            { "metamorphose", "metamorphoses" },
            { "intercourse", "intercourses" },
            { "commonsense", "commonsenses" },
            { "intersperse", "intersperses" },
            { "merchandise", "merchandises" },
            { "phosphatase", "phosphatases" },
            { "summerhouse", "summerhouses" },
            { "watercourse", "watercourses" },
            { "catchphrase", "catchphrases" },
            { "compromise", "compromises" },
            { "greenhouse", "greenhouses" },
            { "lighthouse", "lighthouses" },
            { "paraphrase", "paraphrases" },
            { "mayonnaise", "mayonnaises" },
            { "racecourse", "racecourses" },
            { "apocalypse", "apocalypses" },
            { "courthouse", "courthouses" },
            { "powerhouse", "powerhouses" },
            { "storehouse", "storehouses" },
            { "glasshouse", "glasshouses" },
            { "hypotenuse", "hypotenuses" },
            { "peroxidase", "peroxidases" },
            { "pillowcase", "pillowcases" },
            { "roundhouse", "roundhouses" },
            { "streetwise", "streetwises" },
            { "expertise", "expertises" },
            { "discourse", "discourses" },
            { "warehouse", "warehouses" },
            { "staircase", "staircases" },
            { "workhouse", "workhouses" },
            { "briefcase", "briefcases" },
            { "clubhouse", "clubhouses" },
            { "clockwise", "clockwises" },
            { "concourse", "concourses" },
            { "playhouse", "playhouses" },
            { "turquoise", "turquoises" },
            { "boathouse", "boathouses" },
            { "cellulose", "celluloses" },
            { "epitomise", "epitomises" },
            { "gatehouse", "gatehouses" },
            { "grandiose", "grandioses" },
            { "menopause", "menopauses" },
            { "penthouse", "penthouses" },
            { "racehorse", "racehorses" },
            { "transpose", "transposes" },
            { "almshouse", "almshouses" },
            { "customise", "customises" },
            { "footloose", "footlooses" },
            { "galvanise", "galvanises" },
            { "princesse", "princesses" },
            { "universe", "universes" },
            { "workhorse", "workhorses" }
        };

        private static readonly IDictionary<string, string> wordsEndingWithSisDictionary = new Dictionary<string, string>
        {
            { "analysis", "analyses" },
            { "crisis", "crises" },
            { "basis", "bases" },
            { "atherosclerosis", "atheroscleroses" },
            { "electrophoresis", "electrophoreses" },
            { "psychoanalysis", "psychoanalyses" },
            { "photosynthesis", "photosyntheses" },
            { "amniocentesis", "amniocenteses" },
            { "metamorphosis", "metamorphoses" },
            { "toxoplasmosis", "toxoplasmoses" },
            { "endometriosis", "endometrioses" },
            { "tuberculosis", "tuberculoses" },
            { "pathogenesis", "pathogeneses" },
            { "osteoporosis", "osteoporoses" },
            { "parenthesis", "parentheses" },
            { "anastomosis", "anastomoses" },
            { "peristalsis", "peristalses" },
            { "hypothesis", "hypotheses" },
            { "antithesis", "antitheses" },
            { "apotheosis", "apotheoses" },
            { "thrombosis", "thromboses" },
            { "diagnosis", "diagnoses" },
            { "synthesis", "syntheses" },
            { "paralysis", "paralyses" },
            { "prognosis", "prognoses" },
            { "cirrhosis", "cirrhoses" },
            { "sclerosis", "scleroses" },
            { "psychosis", "psychoses" },
            { "apoptosis", "apoptoses" },
            { "symbiosis", "symbioses" }
        };

        private static readonly IDictionary<string, string> wordsEndingWithSusDictionary = new Dictionary<string, string>
        {
            { "consensus", "consensuses" },
            { "census", "censuses" }
        };

        private static readonly IDictionary<string, string> wordsEndingWithInxAnxYnxDictionary = new Dictionary<string, string>
        {
            { "sphinx", "sphinxes" },
            { "larynx", "larynges" },
            { "lynx", "lynxes" },
            { "pharynx", "pharynxes" },
            { "phalanx", "phalanxes" }
        };

        /// <summary>
        /// .cctor
        /// </summary>
        static EnglishUtils()
        {
            knownPluralWords = new List<string>(
                irregularPluralsDictionary.Values.Concat(assimilatedClassicalInflectionDictionary.Values)
                    .Concat(oSuffixDictionary.Values)
                    .Concat(classicalInflectionDictionary.Values)
                    .Concat(irregularVerbList.Values)
                    .Concat(irregularPluralsDictionary.Values)
                    .Concat(wordsEndingWithSeDictionary.Values)
                    .Concat(wordsEndingWithSisDictionary.Values)
                    .Concat(wordsEndingWithSusDictionary.Values)
                    .Concat(wordsEndingWithInxAnxYnxDictionary.Values)
                    .Concat(uninflectiveWordList))
                .ToArray();
        }

        #region Private methods

        private static string GetSuffixWord(string word, out string prefixWord)
        {
            int num = word.LastIndexOf(' ');
            prefixWord = word.Substring(0, num + 1);
            return word.Substring(num + 1);
        }

        private static string InternalPluralize(string word)
        {
            if (IsNoOpWord(word))
            {
                return word;
            }
            string suffixWord = GetSuffixWord(word, out string str);

            if (IsNoOpWord(suffixWord))
            {
                return string.Concat(str, suffixWord);
            }

            if (IsUninflective(suffixWord))
            {
                return string.Concat(str, suffixWord);
            }

            if (knownPluralWords.Contains(suffixWord.ToLowerInvariant()))
            {
                return string.Concat(str, suffixWord);
            }

            if (irregularPluralsDictionary.ContainsKey(suffixWord))
            {
                return string.Concat(str, irregularPluralsDictionary[suffixWord]);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "man"
            }, s => string.Concat(s.Remove(s.Length - 2, 2), "en"), out string str1))
            {
                return string.Concat(str, str1);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "louse",
                "mouse"
            }, s => string.Concat(s.Remove(s.Length - 4, 4), "ice"), out str1))
            {
                return string.Concat(str, str1);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "tooth"
            }, s => string.Concat(s.Remove(s.Length - 4, 4), "eeth"), out str1))
            {
                return string.Concat(str, str1);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "goose"
            }, s => string.Concat(s.Remove(s.Length - 4, 4), "eese"), out str1))
            {
                return string.Concat(str, str1);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "foot"
            }, s => string.Concat(s.Remove(s.Length - 3, 3), "eet"), out str1))
            {
                return string.Concat(str, str1);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "zoon"
            }, s => string.Concat(s.Remove(s.Length - 3, 3), "oa"), out str1))
            {
                return string.Concat(str, str1);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "cis",
                "sis",
                "xis"
            }, s => string.Concat(s.Remove(s.Length - 2, 2), "es"), out str1))
            {
                return string.Concat(str, str1);
            }

            if (assimilatedClassicalInflectionDictionary.ContainsKey(suffixWord))
            {
                return string.Concat(str, assimilatedClassicalInflectionDictionary[suffixWord]);
            }

            if (classicalInflectionDictionary.ContainsKey(suffixWord))
            {
                return string.Concat(str, classicalInflectionDictionary[suffixWord]);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "trix"
            }, s => string.Concat(s.Remove(s.Length - 1, 1), "ces"), out str1))
            {
                return string.Concat(str, str1);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "eau",
                "ieu"
            }, s => string.Concat(s, "x"), out str1))
            {
                return string.Concat(str, str1);
            }

            if (wordsEndingWithInxAnxYnxDictionary.ContainsKey(suffixWord))
            {
                return string.Concat(str, wordsEndingWithInxAnxYnxDictionary[suffixWord]);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "ch",
                "sh",
                "ss"
            }, s => string.Concat(s, "es"), out str1))
            {
                return string.Concat(str, str1);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "alf",
                "elf",
                "olf",
                "eaf",
                "arf"
            }, s =>
            {
                if (s.ToLower().EndsWith("deaf"))
                {
                    return s;
                }
                return string.Concat(s.Remove(s.Length - 1, 1), "ves");
            }, out str1))
            {
                return string.Concat(str, str1);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "nife",
                "life",
                "wife"
            }, s => string.Concat(s.Remove(s.Length - 2, 2), "ves"), out str1))
            {
                return string.Concat(str, str1);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "ay",
                "ey",
                "iy",
                "oy",
                "uy"
            }, s => string.Concat(s, "s"), out str1))
            {
                return string.Concat(str, str1);
            }

            if (suffixWord.ToLower().EndsWith("y"))
            {
                return string.Concat(str, suffixWord.Remove(suffixWord.Length - 1, 1), "ies");
            }

            if (oSuffixDictionary.ContainsKey(suffixWord))
            {
                return string.Concat(str, oSuffixDictionary[suffixWord]);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new[]
            {
                "ao",
                "eo",
                "io",
                "oo",
                "uo"
            }, s => string.Concat(s, "s"), out str1))
            {
                return string.Concat(str, str1);
            }

            if (suffixWord.ToLower().EndsWith("o") || suffixWord.ToLower().EndsWith("s"))
            {
                return string.Concat(str, suffixWord, "es");
            }

            if (suffixWord.ToLower().EndsWith("x"))
            {
                return string.Concat(str, suffixWord, "es");
            }

            return string.Concat(str, suffixWord, "s");
        }

        private static bool IsAlphabets(string word)
        {
            var trimWord = word.Trim();
            if (!string.IsNullOrEmpty(trimWord) && word.Equals(trimWord) && !Regex.IsMatch(word, "[^a-zA-Z\\s]"))
            {
                return true;
            }
            return false;
        }

        private static bool IsNoOpWord(string word)
        {
            if (IsAlphabets(word) && word.Length > 1 && !pronounList.Contains(word.ToLowerInvariant()))
            {
                return false;
            }
            return true;
        }

        private static bool IsUninflective(string word)
        {
            if (!DoesWordContainSuffix(word, uninflectiveSuffixList) && (word.ToLower().Equals(word) || !word.EndsWith("ese")) &&
                !uninflectiveWordList.Contains(word.ToLowerInvariant()))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// This method indicates whether a word contains a specific suffix.
        /// </summary>
        /// <param name="word">The word to use for the operation.</param>
        /// <param name="suffixes">A list of suffixes to check for.</param>
        /// <returns>True if the word contains one or more of the specified
        /// suffixes; false otherwise.</returns>
        private static bool DoesWordContainSuffix(
            string word,
            IEnumerable<string> suffixes
        )
        {
            return suffixes.Any(s => word.ToLower().EndsWith(s));
        }

        /// <summary>
        /// This method performs an action if a word contains a specific suffix.
        /// </summary>
        /// <param name="word">The word to use for the operation.</param>
        /// <param name="suffixes">A list of suffixes to check for.</param>
        /// <param name="operationOnWord">The action to be performed.</param>
        /// <param name="newWord">The results of the operation.</param>
        /// <returns>True if the operation succeeded; false otherwise.</returns>
        private static bool TryInflectOnSuffixInWord(
            string word,
            string[] suffixes,
            Func<string, string> operationOnWord,
            out string newWord
        )
        {
            newWord = null;
            if (!DoesWordContainSuffix(word, suffixes))
            {
                return false;
            }
            newWord = operationOnWord(word);
            return true;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// The method returns the plural form of the specified word depends on number.
        /// </summary>
        /// <param name="number">The specified number to determine word form.</param>
        /// <param name="singular">Singular form of word.</param>
        /// <param name="plural">Pluaral form of word. If not specified it will be detected automatically.</param>
        /// <returns>The plural form of the specified word.</returns>
        public static string PluralizeWithNumber(int number, string singular, string plural = null)
        {
            if (number > 1)
            {
                return plural ?? Pluralize(singular);
            }

            return Pluralize(singular);
        }

        /// <summary>
        /// The method returns the plural form of the specified word.
        /// </summary>
        /// <param name="singular">Singular form of word.</param>
        /// <returns>The plural form of the specified word.</returns>
        public static string Pluralize(string singular)
        {
            return InternalPluralize(singular);
        }

        #endregion
    }
}
