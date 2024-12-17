using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace FlashcardApp
{
    class Program
    {
        static void Main(string[] args)
        {
            App app = new App();
            app.Run();
        }
    }

    public class App
    {
        private Account currentAccount;
        private int sessionEasy = 0, sessionMedium = 0, sessionHard = 0;
        private DateTime sessionStartTime;

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Flashcard Application");
                Console.WriteLine("1. Create Account");
                Console.WriteLine("2. Log In");
                Console.WriteLine("3. Exit");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateAccount();
                        break;
                    case "2":
                        LogIn();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        private void CreateAccount()
        {
            Console.Write("Enter a username: ");
            string username = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(username) || !IsAlphanumeric(username))
            {
                Console.WriteLine("Error: Username must be alphanumeric. Press Enter to try again.");
                Console.ReadLine();
                return;
            }

            if (File.Exists($"{username}.json"))
            {
                Console.WriteLine("Error: An account with this username already exists. Press Enter to try again.");
                Console.ReadLine();
                return;
            }

            currentAccount = new Account { Username = username };
            currentAccount.SaveToFile();
            Console.WriteLine("Account created successfully! Press Enter to continue.");
            Console.ReadLine();
        }

        private void LogIn()
        {
            Console.Write("Enter your username: ");
            string username = Console.ReadLine();

            if (!File.Exists($"{username}.json"))
            {
                Console.WriteLine("Error: Account not found. Press Enter to return to the menu.");
                Console.ReadLine();
                return;
            }

            try
            {
                currentAccount = Account.LoadFromFile(username);
                Console.WriteLine($"Welcome back, {currentAccount.Username}! Press Enter to continue.");
                Console.ReadLine();
                ManageAccount();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading account: {ex.Message}. Press Enter to return to the menu.");
                Console.ReadLine();
            }
        }

        private void ManageAccount()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Hello, {currentAccount.Username}!");
                Console.WriteLine("1. Manage Decks");
                Console.WriteLine("2. View Statistics");
                Console.WriteLine("3. Revise");
                Console.WriteLine("4. Log Out");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ManageDecks();
                        break;
                    case "2":
                        ViewStatistics();
                        break;
                    case "3":
                        Revise();
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        private void ManageDecks()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Deck Management");
                Console.WriteLine("1. Create a New Deck");
                Console.WriteLine("2. View All Decks");
                Console.WriteLine("3. Rename a Deck");
                Console.WriteLine("4. Delete a Deck");
                Console.WriteLine("5. Manage Flashcards in a Deck");
                Console.WriteLine("6. Return to Main Menu");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateDeck();
                        break;
                    case "2":
                        ViewDecks();
                        break;
                    case "3":
                        RenameDeck();
                        break;
                    case "4":
                        DeleteDeck();
                        break;
                    case "5":
                        if (SelectDeck(out Deck selectedDeck))
                        {
                            ManageFlashcards(selectedDeck);
                        }
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        private void ViewStatistics()
        {
            Console.Clear();
            Console.WriteLine("Statistics");
            Console.WriteLine("-----------------------");

            // Calculate session time, defaulting to 0 if sessionStartTime is uninitialized
            int sessionTimeSpent = sessionStartTime != default
                ? (int)(DateTime.Now - sessionStartTime).TotalMinutes
                : 0;

            // Display session stats
            Console.WriteLine("Session Stats:");
            Console.WriteLine($"- Cards Reviewed: {sessionEasy + sessionMedium + sessionHard}");
            Console.WriteLine($"  Easy: {sessionEasy} | Medium: {sessionMedium} | Hard: {sessionHard}");
            Console.WriteLine($"- Time Spent: {sessionTimeSpent} minutes");
            Console.WriteLine();

            // Display cumulative stats
            Console.WriteLine("Cumulative Stats:");
            Console.WriteLine($"- Total Cards Reviewed: {currentAccount.Stats.TotalCardsReviewed}");
            Console.WriteLine($"  Easy: {currentAccount.Stats.TotalEasy} | Medium: {currentAccount.Stats.TotalMedium} | Hard: {currentAccount.Stats.TotalHard}");
            Console.WriteLine($"- Total Time Spent: {currentAccount.Stats.TotalTimeSpent} minutes");
            Console.WriteLine();

            // Display streak stats
            Console.WriteLine("Streak Stats:");
            Console.WriteLine($"- Current Streak: {currentAccount.Stats.CurrentStreak} days");
            Console.WriteLine($"- Longest Streak: {currentAccount.Stats.LongestStreak} days");
            Console.WriteLine();

            // Deck-specific stats
            Console.WriteLine("Press Enter to view Deck-Specific Statistics.");
            Console.ReadLine();

            ViewDeckStatistics(); // Call deck-specific stats
        }





        private void ViewDeckStatistics()
        {
            Console.Clear();
            Console.WriteLine("Deck-Specific Statistics");
            Console.WriteLine("-------------------------");

            if (currentAccount.Decks.Count == 0)
            {
                Console.WriteLine("No decks available. Press Enter to return.");
                Console.ReadLine();
                return;
            }

            foreach (var deck in currentAccount.Decks)
            {
                Console.WriteLine($"Deck: {deck.Name}");
                Console.WriteLine($"- Total Flashcards: {deck.Flashcards.Count}");
                Console.WriteLine($"- Total Cards Reviewed: {deck.TotalReviewed}");
                Console.WriteLine($"- Difficulty Breakdown: Easy: {deck.EasyCount}, Medium: {deck.MediumCount}, Hard: {deck.HardCount}");
                Console.WriteLine($"- Mastery Percentage: {deck.MasteryPercentage:F2}%");
                Console.WriteLine();
            }

            Console.WriteLine("Press Enter to return.");
            Console.ReadLine();
        }
       





        private void Revise()
        {
            if (currentAccount.Decks.Count == 0)
            {
                Console.WriteLine("No decks available. Please create a deck first. Press Enter to return.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Available Decks:");
            for (int i = 0; i < currentAccount.Decks.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {currentAccount.Decks[i].Name}");
            }

            Console.Write("Select a deck to revise by number: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= currentAccount.Decks.Count)
            {
                Deck selectedDeck = currentAccount.Decks[choice - 1];
                ShowFilterMenu(selectedDeck);
            }
            else
            {
                Console.WriteLine("Invalid selection. Press Enter to return.");
                Console.ReadLine();
            }
        }

        private void ShowFilterMenu(Deck deck)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Revision Options for Deck: {deck.Name}");
                Console.WriteLine("1. Revise All Cards");
                Console.WriteLine("2. Revise Only Hard Cards");
                Console.WriteLine("3. Revise Only Due Cards");
                Console.WriteLine("4. Revise Only Unreviewed Cards");
                Console.WriteLine("5. Return to Main Menu");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine();

                List<Flashcard> filteredFlashcards = null;

                switch (choice)
                {
                    case "1":
                        filteredFlashcards = deck.Flashcards;
                        break;
                    case "2":
                        filteredFlashcards = deck.Flashcards.FindAll(card => card.Difficulty == 3);
                        break;
                    case "3": // Revise Only Due Cards
                        filteredFlashcards = deck.Flashcards.FindAll(card => card.NextReview <= DateTime.Now);
                        if (filteredFlashcards.Count > 0)
                        {
                            filteredFlashcards = MergeSort(filteredFlashcards); // Sort due cards by priority
                        }
                        break;
                    case "4":
                        filteredFlashcards = deck.Flashcards.FindAll(card => card.LastReviewed == DateTime.MinValue);
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Press Enter to try again.");
                        Console.ReadLine();
                        continue;
                }

                StartRevision(deck, filteredFlashcards);
                break;
            }
        }

        private List<Flashcard> MergeSort(List<Flashcard> flashcards)
        {
            if (flashcards.Count <= 1)
                return flashcards;

            // Split the list into halves
            int middle = flashcards.Count / 2;
            List<Flashcard> left = flashcards.GetRange(0, middle);
            List<Flashcard> right = flashcards.GetRange(middle, flashcards.Count - middle);

            // Recursively sort both halves
            left = MergeSort(left);
            right = MergeSort(right);

            // Merge the sorted halves
            return Merge(left, right);
        }

        private List<Flashcard> Merge(List<Flashcard> left, List<Flashcard> right)
        {
            List<Flashcard> result = new List<Flashcard>();
            int leftIndex = 0, rightIndex = 0;

            // Compare and merge elements
            while (leftIndex < left.Count && rightIndex < right.Count)
            {
                if (IsHigherPriority(left[leftIndex], right[rightIndex]))
                {
                    result.Add(left[leftIndex]);
                    leftIndex++;
                }
                else
                {
                    result.Add(right[rightIndex]);
                    rightIndex++;
                }
            }

            // Add remaining elements from both halves
            result.AddRange(left.GetRange(leftIndex, left.Count - leftIndex));
            result.AddRange(right.GetRange(rightIndex, right.Count - rightIndex));

            return result;
        }

        private bool IsHigherPriority(Flashcard a, Flashcard b)
        {
            // Compare by NextReview date first
            if (a.NextReview < b.NextReview)
                return true;
            if (a.NextReview > b.NextReview)
                return false;

            // If dates are equal, compare by Difficulty (Hard > Medium > Easy)
            return a.Difficulty > b.Difficulty;
        }




        private void StartRevision(Deck deck, List<Flashcard> flashcards)
        {
            sessionEasy = 0;
            sessionMedium = 0;
            sessionHard = 0;
            sessionStartTime = DateTime.Now;
           

            Console.Clear();
            Console.WriteLine($"Starting revision session for Deck: {deck.Name}");
            Console.WriteLine("Press Enter to begin.");
            Console.ReadLine();

            if (flashcards.Count == 0)
            {
                Console.WriteLine("No cards match your criteria. Press Enter to return.");
                Console.ReadLine();
                return;
            }

            foreach (var flashcard in flashcards)
            {
                Console.Clear();
                Console.WriteLine($"Question: {flashcard.Question}");
                Console.WriteLine("Press Enter to reveal the answer.");
                Console.ReadLine();
                Console.WriteLine($"Answer: {flashcard.Answer}");

                int difficulty = GetDifficultyRating();
                UpdateFlashcardProperties(flashcard, difficulty);

                currentAccount.SaveToFile(); // Save progress after each card
            }

            // Calculate session time spent
            int sessionTimeSpent = (int)(DateTime.Now - sessionStartTime).TotalMinutes;

            // Update cumulative statistics
            currentAccount.Stats.AddSessionStats(sessionEasy, sessionMedium, sessionHard, sessionTimeSpent);

            // Update streak stats
            currentAccount.Stats.UpdateStreak();
            currentAccount.SaveToFile();

            Console.WriteLine("Revision session complete! Streak stats have been updated.");
            Console.WriteLine($"Current Streak: {currentAccount.Stats.CurrentStreak} days");
            Console.WriteLine($"Longest Streak: {currentAccount.Stats.LongestStreak} days");
            Console.WriteLine("Press Enter to return to the menu.");
            Console.ReadLine();
        }



        private int GetDifficultyRating()
        {
            while (true)
            {
                Console.WriteLine("How was this card?");
                Console.WriteLine("1. Easy");
                Console.WriteLine("2. Medium");
                Console.WriteLine("3. Hard");
                Console.Write("Enter your choice: ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int difficulty) && difficulty >= 1 && difficulty <= 3)
                {
                    return difficulty;
                }

                Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.");
            }
        }

        private void UpdateFlashcardProperties(Flashcard flashcard, int difficulty)
        {
            flashcard.LastReviewed = DateTime.Now;

            // Adaptive Spaced Repetition
            double rating = difficulty == 1 ? 5 : (difficulty == 2 ? 3 : 1);

            // Update Easiness Factor
            flashcard.EasinessFactor = Math.Max(1.3, flashcard.EasinessFactor + (0.1 - (5 - rating) * (0.08 + (5 - rating) * 0.02)));

            // Update Interval and Repetitions
            if (rating >= 3) // Successful review
            {
                flashcard.Repetitions++;
                if (flashcard.Repetitions == 1)
                {
                    flashcard.Interval = 1;
                }
                else if (flashcard.Repetitions == 2)
                {
                    flashcard.Interval = 6;
                }
                else
                {
                    flashcard.Interval = (int)Math.Round(flashcard.Interval * flashcard.EasinessFactor);
                }
            }
            else // Failed review
            {
                flashcard.Repetitions = 0; // Reset repetitions
                flashcard.Interval = 1; // Reset interval
            }

            // Set NextReview
            flashcard.NextReview = DateTime.Now.AddDays(flashcard.Interval);

            // Update cumulative statistics
            Deck deck = currentAccount.Decks.Find(d => d.Flashcards.Contains(flashcard));
            if (deck != null)
            {
                deck.TotalReviewed++;
                switch (difficulty)
                {
                    case 1: deck.EasyCount++; sessionEasy++; break;
                    case 2: deck.MediumCount++; sessionMedium++; break;
                    case 3: deck.HardCount++; sessionHard++; break;
                }
            }

            flashcard.Difficulty = difficulty; // Save difficulty for filtering
        }



        private bool IsAlphanumeric(string str)
        {
            foreach (char c in str)
            {
                if (!char.IsLetterOrDigit(c))
                    return false;
            }
            return true;
        }

        private bool SelectDeck(out Deck deck)
        {
            deck = null;

            if (currentAccount.Decks.Count == 0)
            {
                Console.WriteLine("No decks available. Press Enter to return.");
                Console.ReadLine();
                return false;
            }

            Console.WriteLine("Your Decks:");
            for (int i = 0; i < currentAccount.Decks.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {currentAccount.Decks[i].Name}");
            }

            Console.Write("Select a deck by number: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= currentAccount.Decks.Count)
            {
                deck = currentAccount.Decks[choice - 1];
                return true;
            }

            Console.WriteLine("Invalid selection. Press Enter to return.");
            Console.ReadLine();
            return false;
        }

        private void CreateDeck()
        {
            Console.Write("Enter a name for the new deck (1–30 characters, alphabetical only): ");
            string deckName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(deckName) || deckName.Length > 30 || !IsAlphabetical(deckName))
            {
                Console.WriteLine("Error: Deck name must be 1–30 alphabetical characters. Press Enter to try again.");
                Console.ReadLine();
                return;
            }

            if (currentAccount.DeckExists(deckName))
            {
                Console.WriteLine("Error: A deck with this name already exists. Press Enter to try again.");
                Console.ReadLine();
                return;
            }

            currentAccount.AddDeck(new Deck { Name = deckName });
            currentAccount.SaveToFile();
            Console.WriteLine($"Deck '{deckName}' created successfully! Press Enter to continue.");
            Console.ReadLine();
        }

        private void ViewDecks()
        {
            if (currentAccount.Decks.Count == 0)
            {
                Console.WriteLine("No decks available. Press Enter to return.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Your Decks:");
            for (int i = 0; i < currentAccount.Decks.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {currentAccount.Decks[i].Name}");
            }
            Console.WriteLine("Press Enter to return.");
            Console.ReadLine();
        }

        private void RenameDeck()
        {
            if (!SelectDeck(out Deck deck))
                return;

            Console.Write("Enter the new name for the deck (1–30 characters, alphabetical only): ");
            string newName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(newName) || newName.Length > 30 || !IsAlphabetical(newName))
            {
                Console.WriteLine("Error: Deck name must be 1–30 alphabetical characters. Press Enter to try again.");
                Console.ReadLine();
                return;
            }

            if (!deck.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && currentAccount.DeckExists(newName))
            {
                Console.WriteLine("Error: A deck with this name already exists. Press Enter to try again.");
                Console.ReadLine();
                return;
            }

            deck.Name = newName;
            currentAccount.SaveToFile();
            Console.WriteLine("Deck renamed successfully! Press Enter to continue.");
            Console.ReadLine();
        }

        private void DeleteDeck()
        {
            if (!SelectDeck(out Deck deck))
                return;

            Console.Write($"Are you sure you want to delete '{deck.Name}'? (Y/N): ");
            string confirmation = Console.ReadLine();
            if (confirmation?.ToUpper() != "Y")
            {
                Console.WriteLine("Operation canceled. Press Enter to return.");
                Console.ReadLine();
                return;
            }

            currentAccount.RemoveDeck(deck);
            currentAccount.SaveToFile();
            Console.WriteLine("Deck deleted successfully! Press Enter to continue.");
            Console.ReadLine();
        }

        private bool IsAlphabetical(string str)
        {
            foreach (char c in str)
            {
                if (!char.IsLetter(c))
                    return false;
            }
            return true;
        }

        private void ManageFlashcards(Deck deck)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Flashcard Management for Deck: {deck.Name}");
                Console.WriteLine("-------------------------------------------");
                Console.WriteLine("1. Add a Flashcard");
                Console.WriteLine("2. View All Flashcards");
                Console.WriteLine("3. Edit a Flashcard");
                Console.WriteLine("4. Delete a Flashcard");
                Console.WriteLine("5. Return to Deck Menu");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddFlashcard(deck);
                        break;
                    case "2":
                        ViewFlashcards(deck);
                        break;
                    case "3":
                        EditFlashcard(deck);
                        break;
                    case "4":
                        DeleteFlashcard(deck);
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        private void AddFlashcard(Deck deck)
        {
            Console.Write("Enter the question for the flashcard: ");
            string question = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(question))
            {
                Console.WriteLine("Error: Question cannot be empty. Press Enter to try again.");
                Console.ReadLine();
                return;
            }

            Console.Write("Enter the answer for the flashcard: ");
            string answer = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(answer))
            {
                Console.WriteLine("Error: Answer cannot be empty. Press Enter to try again.");
                Console.ReadLine();
                return;
            }

            deck.Flashcards.Add(new Flashcard
            {
                Question = question,
                Answer = answer,
                Difficulty = 2, // Default difficulty is Medium
                LastReviewed = DateTime.MinValue,
                NextReview = DateTime.Now
            });

            currentAccount.SaveToFile();
            Console.WriteLine("Flashcard added successfully! Press Enter to continue.");
            Console.ReadLine();
        }

        private void ViewFlashcards(Deck deck)
        {
            if (deck.Flashcards.Count == 0)
            {
                Console.WriteLine("No flashcards in this deck. Press Enter to return.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Flashcards:");
            for (int i = 0; i < deck.Flashcards.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {deck.Flashcards[i].Question} (Answer: {deck.Flashcards[i].Answer})");
            }
            Console.WriteLine("Press Enter to return.");
            Console.ReadLine();
        }

        private void EditFlashcard(Deck deck)
        {
            if (!SelectFlashcard(deck, out Flashcard flashcard))
                return;

            Console.WriteLine($"Current Question: {flashcard.Question}");
            Console.Write("Enter a new question (leave blank to keep the current question): ");
            string newQuestion = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(newQuestion))
            {
                flashcard.Question = newQuestion;
            }

            Console.WriteLine($"Current Answer: {flashcard.Answer}");
            Console.Write("Enter a new answer (leave blank to keep the current answer): ");
            string newAnswer = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(newAnswer))
            {
                flashcard.Answer = newAnswer;
            }

            currentAccount.SaveToFile();
            Console.WriteLine("Flashcard updated successfully! Press Enter to continue.");
            Console.ReadLine();
        }

        private void DeleteFlashcard(Deck deck)
        {
            if (!SelectFlashcard(deck, out Flashcard flashcard))
                return;

            Console.Write($"Are you sure you want to delete this flashcard? (Y/N): ");
            string confirmation = Console.ReadLine();
            if (confirmation?.ToUpper() != "Y")
            {
                Console.WriteLine("Operation canceled. Press Enter to return.");
                Console.ReadLine();
                return;
            }

            deck.Flashcards.Remove(flashcard);
            currentAccount.SaveToFile();
            Console.WriteLine("Flashcard deleted successfully! Press Enter to continue.");
            Console.ReadLine();
        }

        private bool SelectFlashcard(Deck deck, out Flashcard flashcard)
        {
            flashcard = null;

            if (deck.Flashcards.Count == 0)
            {
                Console.WriteLine("No flashcards in this deck. Press Enter to return.");
                Console.ReadLine();
                return false;
            }

            Console.WriteLine("Flashcards:");
            for (int i = 0; i < deck.Flashcards.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {deck.Flashcards[i].Question}");
            }

            Console.Write("Select a flashcard by number: ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= deck.Flashcards.Count)
            {
                flashcard = deck.Flashcards[choice - 1];
                return true;
            }

            Console.WriteLine("Invalid selection. Press Enter to return.");
            Console.ReadLine();
            return false;
        }
    }

    public class Account
    {
        public string Username { get; set; }
        public List<Deck> Decks { get; set; } = new List<Deck>();
        public Statistics Stats { get; set; } = new Statistics();

        public void SaveToFile()
        {
            string json = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText($"{Username}.json", json);
        }

        public static Account LoadFromFile(string username)
        {
            string json = File.ReadAllText($"{username}.json");
            return JsonConvert.DeserializeObject<Account>(json);
        }

        public void AddDeck(Deck deck)
        {
            Decks.Add(deck);
        }

        public void RemoveDeck(Deck deck)
        {
            Decks.Remove(deck);
        }

        public bool DeckExists(string name)
        {
            return Decks.Exists(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class Deck
    {
        public string Name { get; set; }
        public List<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
        public int TotalReviewed { get; set; } // Total cards reviewed in this deck
        public int EasyCount { get; set; } // Cards rated Easy
        public int MediumCount { get; set; } // Cards rated Medium
        public int HardCount { get; set; } // Cards rated Hard

        public double MasteryPercentage
        {
            get
            {
                return Flashcards.Count > 0
                    ? (double)EasyCount / Flashcards.Count * 100
                    : 0;
            }
        }
    }


    public class Flashcard
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public int Difficulty { get; set; } // 1 = Easy, 2 = Medium, 3 = Hard
        public DateTime LastReviewed { get; set; }
        public DateTime NextReview { get; set; }
        public double EasinessFactor { get; set; } = 2.5; // Adaptive Spaced Repetition
        public int Interval { get; set; } = 1; // Days until next review
        public int Repetitions { get; set; } = 0; // Number of successful reviews
    }


    public class Statistics
    {
        public int TotalCardsReviewed { get; set; } // All-time cards reviewed
        public int TotalEasy { get; set; }
        public int TotalMedium { get; set; }
        public int TotalHard { get; set; }
        public int TotalTimeSpent { get; set; } // In minutes

        public void AddSessionStats(int easy, int medium, int hard, int timeSpent)
        {
            TotalCardsReviewed += (easy + medium + hard);
            TotalEasy += easy;
            TotalMedium += medium;
            TotalHard += hard;
            TotalTimeSpent += timeSpent;
        }


        // Streak Tracking
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime LastRevisionDate { get; set; } = DateTime.MinValue; // Defaults to never revised

        public void UpdateStreak()
        {
            // Calculate if today is consecutive with the last revision
            if (LastRevisionDate.Date == DateTime.Today.AddDays(-1)) // Yesterday
            {
                CurrentStreak++;
            }
            else if (LastRevisionDate.Date != DateTime.Today) // Missed a day
            {
                CurrentStreak = 1; // Reset streak to 1
            }

            // Update longest streak
            if (CurrentStreak > LongestStreak)
            {
                LongestStreak = CurrentStreak;
            }

            // Update last revision date
            LastRevisionDate = DateTime.Today;
        }

    }

}
