using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrequencyDict
{
    class Program
    {
        static void Main(string[] args)
        {
            string text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer imperdiet est quis elit viverra, vel tincidunt justo condimentum. Nullam vulputate lorem est, id semper tortor fermentum nec. Nulla vel orci eu eros ultrices elementum dictum in mi. Aliquam erat volutpat. Ut lorem ipsum, consequat at dignissim quis, vulputate ac neque. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent non egestas massa. Suspendisse vestibulum ex in mi luctus tempus. Donec vitae pharetra massa, at suscipit justo. Nulla tempor, felis a ornare molestie, est ligula consequat massa, quis porta urna risus id urna. Integer ut lorem eu risus tincidunt cursus. Curabitur molestie urna lectus, id malesuada sem sodales at. Sed id mi condimentum, fringilla urna hendrerit, ornare dolor.";
            string text2 = "a b c d e f g h i g k l m n o  p q r s t u v w x y z t";

            var model = new FrequencyDictionaryModel();
            var splittedText = model.TextProcessing(text);
            for (int i = 0; i < 22; i++)
            {
                splittedText.AddRange(splittedText);
            }
            var prepText = model.SplitTextByChunks(splittedText, 5);
            

            //model.BuildLargeTree(prepText);
            model.InsertWords(splittedText);


            //Console.WriteLine(model.GetAllWords().Count);
            

            foreach (var item in model.GetUniqueWords())
            {
                Console.WriteLine(item);
            }


            Console.ReadLine();
        }
    }

    class FrequencyDictionaryModel
    {
        private DictLeaf Root = new DictLeaf(' ');



        // <DictLeaf>
        private class DictLeaf
        {
            // values
            public Dictionary<char, DictLeaf> Leafs { get; set; }
            private char LeafVal { get; set; }
            public int Amount { get; set; } = 0;

            // constructor
            public DictLeaf(char thisLeaf)
            {
                LeafVal = thisLeaf;
                Leafs = new Dictionary<char, DictLeaf>();
            }

            // insert new value
            public void InsertValue(char currVal, string stringValue)
            {
                // if current char of word-to-insert compares to char into this leaf
                if (currVal != LeafVal)
                {
                    throw new Exception(); //todo()
                }

                
                if (stringValue.Length == 0)    // if this char is last in word-to-insert
                {
                    Amount++;
                }
                else
                {
                    char nextVal = stringValue[0];
                    if (!Leafs.ContainsKey(nextVal))    // if there wasnt such tree-way before (creating new path)
                    {
                        Leafs.Add(nextVal, new DictLeaf(nextVal));
                    }
                    Leafs[nextVal].InsertValue(nextVal, stringValue.Substring(1));  // pushing remaining part of word-to-insert further to path
                }
            }
            
            // search if value added to dict and return amount of this value
            public int GetAmount(char currVal, string stringValue)
            {
                // if current char of word-to-insert compares to char into this leaf
                if (currVal != LeafVal)
                {
                    throw new Exception(); //todo()
                }

                if (stringValue.Length == 0)    // if this char is last in word-to-insert
                {
                    return Amount;
                }
                else
                {
                    char nextVal = stringValue[0];
                    if (Leafs.ContainsKey(nextVal))    // if further path exist -> continue search deeper
                    {
                        return Leafs[nextVal].GetAmount(nextVal, stringValue.Substring(1));
                    }
                    else
                    {
                        return 0;   // there is no such word in dict
                    }
                }
            }

            // returns char of this leaf
            public char GetRoot() {
                return LeafVal;
            }

            // getting word of this and all parents
            public List<string> GetWords(string currWord)
            {
                currWord = currWord + LeafVal;
                List<string> toRet = new List<string>();

                if(Amount != 0)
                {
                    for(int i = 0; i < Amount; i++)
                    {
                        toRet.Add(currWord);
                    }
                }

                foreach (KeyValuePair<char, DictLeaf> leaf in Leafs)
                {
                    toRet.AddRange(leaf.Value.GetWords(currWord));
                }
                return toRet;
            }

            // getting unique words with its amounts of this and all parents
            public List<string> GetUniqueWords(string currWord)
            {
                currWord = currWord + LeafVal;
                List<string> toRet = new List<string>();

                if (Amount != 0)
                {
                    toRet.Add($"{currWord} {Amount}");
                    
                }

                foreach (KeyValuePair<char, DictLeaf> leaf in Leafs)
                {
                    toRet.AddRange(leaf.Value.GetUniqueWords(currWord));
                }
                return toRet;
            }
            
        }
        // </DictLeaf>



        // building new tree with given words
        private DictLeaf BuildTree(List<string> words)
        {
            DictLeaf toRet = new DictLeaf(' ');
            foreach (string word in words)
            {
                toRet.InsertValue(' ', word);
            }
            return toRet;
        }

        // summarize lists of leafs
        private DictLeaf SummarizeLeafs(List<DictLeaf> leafs)
        {
            char currChar = leafs[0].GetRoot();
            DictLeaf toRet = new DictLeaf(currChar);
            Dictionary<char, List<DictLeaf>> wtf = new Dictionary<char, List<DictLeaf>>();

            foreach (var item in leafs)
            {
                if(item.GetRoot() != currChar)
                {
                    throw new Exception(); // todo
                }

                toRet.Amount += item.Amount;

                foreach (var dictItem in item.Leafs)
                {
                    if(wtf.TryGetValue(dictItem.Key, out var lol))
                    {
                        wtf[dictItem.Key].Add(dictItem.Value);
                    }
                    else
                    {
                        wtf.Add(dictItem.Key, new List<DictLeaf>() { dictItem.Value });
                    }
                }
            }


            Dictionary<char, DictLeaf> toSetAsRoot = new Dictionary<char, DictLeaf>();

            foreach (var item in wtf)
            {
                toSetAsRoot.Add(item.Key, SummarizeLeafs(item.Value));
            }
            toRet.Leafs = toSetAsRoot;
            return toRet;
        }
        

        // adding list of words to dict-tree
        public void InsertWords(List<string> wordsToInsert)
        {
            foreach (string word in wordsToInsert)
            {
                Root.InsertValue(' ', word.ToLower());
            }
        }

        // adding one word to dict-tree
        public void InsertWord(string wordToInsert)
        {
            Root.InsertValue(' ', wordToInsert.ToLower());
        }

        // search for amount of given word in dict-tree
        public int GetAmountOfWord(string wordToSearch)
        {
            return Root.GetAmount(' ', wordToSearch.ToLower());
        }

        // getting list of all words in tree (with repeating if many)
        public List<string> GetAllWords()
        {
            List<string> toRet = new List<string>();
            foreach (string word in Root.GetWords(""))
            {
                toRet.Add(word.Trim());
            }
            return toRet;
        }

        // getting list of unique words in tree with amount of repeating them
        public List<string> GetUniqueWords()
        {
            List<string> toRet = new List<string>();
            foreach (string word in Root.GetUniqueWords(""))
            {
                toRet.Add(word.Trim());
            }
            return toRet;
        }

        // working with raw text
        public List<string> TextProcessing(string rawText)
        {
            rawText = rawText.Replace('.', ' ').Replace(',', ' ').Replace('-', ' ').Replace('?', ' ').Replace('!', ' ').ToLower();
            while(rawText.IndexOf("  ") != -1)
            {
                rawText = rawText.Replace("  ", " ");
            }
            return rawText.Split().ToList();
        }

        // splitting data on chunks 
        public List<List<string>> SplitTextByChunks(List<string> text, int amount)
        {
            var toRet = new List<List<string>>();
            for(int i = 0; i < amount; i++)
            {
                toRet.Add(new List<string>());
            }

            for (int i = 0; i < text.Count; i++)
            {
                toRet[(int)(i % amount)].Add(text[i]);
            }


            return toRet;
        }
        
        // build big model by splitting data, building small ones and summing them up
        private DictLeaf BuildRootOfLargeTree(List<List<string>> wordsChunks)
        {

            int amount = wordsChunks.Count;
            List<DictLeaf> dictsSet = new List<DictLeaf>();
            for (int i = 0; i < amount; i++)
            {
                dictsSet.Add(Task.Run(() => BuildTree(wordsChunks[i])).Result);
            }
            DictLeaf toRet = SummarizeLeafs(dictsSet);
            
            return toRet;

        }//*/

        // setting builded model to root
        public void BuildLargeTree(List<List<string>> wordsChunks)
        {
            Root = BuildRootOfLargeTree(wordsChunks);
        }
    }

    
}
