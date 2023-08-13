using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public enum Genre { STATIC, DYNAMIC }
public enum DynamicAction { NONE, TURN, SEEK }

namespace LaserGestures {
    [Serializable()]
    public class GestureSet {

        List<Gesture> gestureList;

        public GestureSet() {
            gestureList = new List<Gesture>();
        }

        public static GestureSet Open(string path) {
                Stream stream = File.Open(path, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();

                GestureSet obj = (GestureSet)formatter.Deserialize(stream);
                stream.Close();
                return obj;
        }

        public void Save(string path) {
            Stream stream = File.Open(path, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, this);
            stream.Close();
        }

        public Boolean Add(Gesture gesture) {
            foreach (Gesture g in gestureList) {
                if (g.Equals(gesture))
                    return false;
            }
            gestureList.Add(gesture);
            return true;
        }

        public Boolean AddStatic(string s) {
            foreach (Gesture g in gestureList) {
                if (g.String.Equals(s) && g.Genre == Genre.STATIC)
                    return false;
            }
            gestureList.Add(new Gesture(s));
            return true;
        }

        public Boolean AddDynamic(string s, DynamicAction da) {
            foreach (Gesture g in gestureList) {
                if (g.String.Equals(s) && g.Genre == Genre.DYNAMIC)
                    return false;
            }
            gestureList.Add(new Gesture(s, da));
            return true;
        }

        public Boolean RemoveStatic(string s) {
            foreach (Gesture g in gestureList) {
                if (g.String.Equals(s) && g.Genre == Genre.STATIC) {
                    gestureList.Remove(g);
                    return true;
                }
            }
            return false;
        }

        public Boolean RemoveDynamic(string s) {
            foreach (Gesture g in gestureList) {
                if (g.String.Equals(s) && g.Genre == Genre.DYNAMIC) {
                    gestureList.Remove(g);
                    return true;
                }
            }
            return false;
        }

        public void Clear() {
            gestureList.Clear();
        }


        public List<Gesture> GetSet {
            get { return new List<Gesture>(gestureList); }
        }


        public Gesture Contains(string s, Genre genre) {
            Gesture g = null;

            if (gestureList.Count == 0)
                return g;

            int[] scores = new int[gestureList.Count];
            for(int i = 0; i < gestureList.Count ; i++) {
                g = gestureList[i];
                scores[i] = int.MaxValue;
                if (g.Genre == genre) {
                    if (g.String.Equals(s))
                        return g;
                    scores[i] = Levenshtein(s, g.String);
                    //Debug.WriteLine("s: {0} \t target: {1} \t ---> score: {2}", s, g.String, scores[i]);
                }
            }
                
            //we didn't find an exact match so lets test the edit distances we just computed
            int minDistance = scores.Min();
            int j = Array.IndexOf(scores, minDistance);
            g = gestureList[j];

            //give the user a chance of one mistake in a gesture between 4 to 5 units
            if (g.String.Length >= 4 && g.String.Length <= 5 && minDistance == 1)
                return g;

            //give the user a chance of two mistakes in a gesture 6 segments or more
            if (g.String.Length >= 6 && minDistance <= 2)
                return g;


            //user wasn't close to matching anything in our set, return null
            return null;
        }


       

        public static int Levenshtein(string s, string t) {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
                return m;
            if (m == 0)
                return n;
            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++) {
                for (int j = 1; j <= m; j++) {
                    //int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int cost = headingCost(t[j - 1], s[i - 1]);
                    d[i, j] = minimum(  d[i - 1, j] + 1,            //deletion
                                        d[i, j - 1] + 1,            //insertion
                                        d[i - 1, j - 1] + cost      //substitution
                                        );
                }
            }
            return d[n, m];
        }


        private static int minimum(int a, int b, int c) {
            int min = a;
            if (b < min)
                min = b;
            if (c < min)
                min = c;
            //if (min == a)
            //    Debug.WriteLine("deletion");
            //if (min == b)
            //    Debug.WriteLine("insertion");
            //if (min == c)
            //    Debug.WriteLine("substition");
            return min;
        }
        

        private static int headingCost(char a, char b) {
            String headings = "U9R3D1L7";
            int distance = Math.Abs(headings.IndexOf(a) - headings.IndexOf(b));
            if (distance > 4)
                distance = 8-distance;
            return distance;
        }



    }//end of GestureSet



    [Serializable()]
    public class Gesture {
        private string gestureString;
        private Genre genre;
        private DynamicAction action;

        public Gesture(string s) {
            gestureString = s;
            genre = Genre.STATIC;
        }

        public Gesture(string s, DynamicAction da) {
            gestureString = s;
            genre = Genre.DYNAMIC;
            action = da;
        }

        public string String {
            get { return gestureString; }
        }

        public Genre Genre {
            get { return genre; }
        }

        public DynamicAction Action {
            get { return action; }
        }

        public bool Equals(Gesture g) {
            return (g.String.Equals(gestureString) && g.Genre == genre) ? true : false;
        }

    }//end of Gesture

}
