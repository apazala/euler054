 internal enum SuitEnum
    {
        H,
        C,
        S,
        D
    }

    internal class Card : IComparable<Card>
    {
        public int Value { get; set; }
        public const int Ace = 14;
        public const int King = 13;
        public const int Queen = 12;
        public const int Jack = 11;
        public SuitEnum Suit { get; set; }

        public static string Symbols = "?*23456789TJQKA";

        public int CompareTo(Card other)
        {
            return Value - other.Value;
        }

        public override string ToString()
        {
            return Symbols[Value] + Suit.ToString();
        }
    }

    internal class Player : IComparable<Player>
    {


        private List<Card> cards = new List<Card>(5);
        private long handValue = -1;
        private HandEnum hand = HandEnum.InvalidHand;
        public long HandValue => ComputeHandValue();

        public HandEnum Hand => ComputeHand();

        public enum HandEnum
        {
            HighCard,
            OnePair,
            TwoPairs,
            ThreeOfAKind,
            Straight,
            Flush,
            FullHouse,
            FourOfAKind,
            StraightFlush,
            RoyalFlush,
            InvalidHand
        }
        // RF  SF  4K  FH  FL  ST  3K  P2 P1  H5 H4 H3 H2 H1
        //[xx][xx][xx][xx][xx][xx][xx][xx|xx][xx|xx|xx|xx|xx]
        //[52][48][44][40][36][32][28][24|20][16|12|08|04|00]
        private static int[] bitcheckShift =
            Enumerable.Range(0, (int)HandEnum.RoyalFlush + 1).Select(h => 16 + 4 * h).ToArray();
        public void AddCard(string cardString)
        {
            SuitEnum suit;
            int val = Card.Symbols.IndexOf(cardString[0]);
            if (val < 2 || !Enum.TryParse(cardString.Substring(1), out suit))
                throw new ArgumentOutOfRangeException();

            cards.Add(new Card { Value = val, Suit = suit });
        }

        public void Clear()
        {
            cards.Clear();
            handValue = -1;
            hand = HandEnum.InvalidHand;
        }

        public int CompareTo(Player other)
        {
            long diff = HandValue - other.HandValue;
            if (diff > 0)
                return 1;
            else if (diff < 0)
                return -1;

            return 0;

        }

        private long ComputeHandValue()
        {
            // RF  SF  4K  FH  FL  ST  3K  P2 P1  H5 H4 H3 H2 H1
            //[xx][xx][xx][xx][xx][xx][xx][xx|xx][xx|xx|xx|xx|xx]
            //[52][48][44][40][36][32][28][24|20][16|12|08|04|00]
            if (handValue == -1)
            {
                bool isFlush = true;
                bool isStraight = true;
                handValue = 0L;
                int[] shift = { 0, 20, 28, 44 };
                int count = 1;
                cards.Sort();
                FixAceStraight();
                for (int i = 1; i < cards.Count; i++)
                {

                    if (cards[i].Value != cards[i - 1].Value)
                    {
                        if (count > 0)
                        {
                            handValue += (long)cards[i - 1].Value << shift[count - 1];
                            shift[count - 1] += 4;
                            count = 1;
                        }

                    }
                    else
                    {
                        count++;
                    }

                    if (cards[i].Suit != cards[i - 1].Suit)
                        isFlush = false;

                    if (isStraight && cards[i].Value - cards[i - 1].Value != 1)
                        isStraight = false;
                }

                handValue += (long)cards[cards.Count - 1].Value << shift[count - 1];
                shift[count - 1] += 4;
                //Full House check
                if (shift[1] > bitcheckShift[(int)HandEnum.OnePair] &&
                    shift[2] > bitcheckShift[(int)HandEnum.ThreeOfAKind])
                    handValue += 0xfL << bitcheckShift[(int)HandEnum.FullHouse];

                if (isFlush)
                    if (isStraight)
                        if (cards[cards.Count - 1].Value == Card.Ace)
                            handValue += 0xfL << bitcheckShift[(int)HandEnum.RoyalFlush];
                        else
                            handValue += 0xfL << bitcheckShift[(int)HandEnum.StraightFlush];
                    else
                        handValue += 0xfL << bitcheckShift[(int)HandEnum.Flush];
                else if (isStraight)
                    handValue += 0xfL << bitcheckShift[(int)HandEnum.Straight];


            }


            return handValue;
        }

        private void FixAceStraight()
        {
            if (cards[0].Value != 2 || cards[4].Value != Card.Ace || cards[1].Value != 3 || cards[2].Value != 4 || cards[3].Value != 5)
                return;

            cards[4].Value = 1;

            cards.Sort();
        }

        private HandEnum ComputeHand()
        {
            if (hand != HandEnum.InvalidHand)
                return hand;

            hand = HandEnum.HighCard;
            for (int h = (int)HandEnum.RoyalFlush; h > 0; h--)
            {
                if (((15L << bitcheckShift[h]) & HandValue) != 0)
                {
                    hand = (HandEnum)h;
                    break;
                }
            }

            return hand;
        }

        public override string ToString()
        {
            return cards.Aggregate("", (s, card) => s + " " + card) + ". Hand: " + Hand;
        }
    }

    class Program
    {

        static void Main()
        {
            Player player1 = new Player();
            Player player2 = new Player();


            int count = 0;
            foreach (string line in System.IO.File.ReadLines(@"/home/alberto/Documents/projecteuler/euler054/p054_poker.txt"))
            {
                player1.Clear();
                player2.Clear();

                string[] cardsStrings = line.Split(' ');
                int i = 0;
                while (i < 5)
                    player1.AddCard(cardsStrings[i++]);
                while (i < 10)
                    player2.AddCard(cardsStrings[i++]);

                bool player1wins = (player1.CompareTo(player2) > 0);

                if (player1wins)
                    count++;

            }
                
            Console.WriteLine($"Player 1 wins {count} times");

            return;
             
        }
    }