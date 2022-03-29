using System;
using System.Xml;

namespace green_house_gases_project
{
    class Program
    {
        // located in bin/Debug/net5.0
        const string XML_FILE = @"ghg-canada.xml";

        static void Main(string[] args)
        {
            // if there is an exception only print it, then exit
            bool threwException = false;
            // initial prompt conditional
            bool exit = false;
            // initial prompt value holder
            string menuChoice;

            try
            {
                // initialize the DOM object
                XmlDocument doc = new XmlDocument();
                doc.Load(XML_FILE);

                while (exit == false)
                {
                    DisplayPromptText();

                    menuChoice = Console.ReadLine().ToUpper();

                    switch (menuChoice)
                    {
                        case "R":
                            DisplayRegions(doc);
                            break;
                        case "S":
                            DisplayDescriptions(doc);
                            break;
                        case "X":
                            exit = true;
                            break;
                        default:
                            Console.Write("Enter either: 'R', 'S', 'X'\n\n");
                            break;
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.Message);
                threwException = true;
            }
            // if there was an exception, just display it no need to display "All done" aswell
            if (!threwException) Console.WriteLine("\nAll done");
        }

        // function only to display prompt text
        public static void DisplayPromptText()
        {
            Console.WriteLine("Greenhouse Gas Emissions in Canada");
            Console.WriteLine("==================================\n");
            Console.WriteLine("'R' to adjust range of years AND to select a region");
            Console.WriteLine("'S' to adjust range of years AND select a specific GHG source");
            Console.WriteLine("'X' to exit the program");
            Console.Write("\nYour selection: ");
        }

        // function for setting the two years 
        public static int[] SetYears(int[] years)
        {
            bool done = false;

            while (done == false)
            {
                do {
                    Console.Write("\nEnter starting year (1990 to 2019): ");
                    years[0] = Convert.ToInt32(Console.ReadLine());
                    if (years[0] < 1990 || years[0] > 2019) Console.WriteLine("Years must be between 1990 and 2019");
                } while (years[0] < 1990 || years[0] > 2019);
                do {
                    Console.Write("Enter ending year (1990 to 2019): ");
                    years[1] = Convert.ToInt32(Console.ReadLine());
                    if (years[1] < 1990 || years[1] > 2019) Console.WriteLine("Years must be between 1990 and 2019");
                    else if (years[1] - years[0] > 4 || years[0] - years[1] > 4) Console.WriteLine("ERROR: Years cannot be more than 4 years apart");
                    else if (years[1] < years[0]) Console.WriteLine("ERROR: Second year must be greater than first year");
                } while (years[1] < 1990 || years[1] > 2019 || years[1] - years[0] > 4 || years[0] - years[1] > 4 || years[1] < years[0]);
                done = true;
                Console.WriteLine($"\nYears set ({years[0]} - {years[1]})\n\n");
            }
            return years;
        }

        // function to select and display regions and their information
        private static void DisplayRegions(XmlDocument doc)
        {
            // array to hold value of year range
            int[] yearsEntered = new int[2];

            SetYears(yearsEntered);

            // get a NodeList of all region elements
            XmlNodeList regionNodes = doc.GetElementsByTagName("region");
            Console.WriteLine(Environment.NewLine + "Select a region by number as shown below...");
            int count = 0;

            // get all region names
            XmlNodeList regionList = doc.SelectNodes("/ghg-canada//region/@name");

            // loop through all region elements in the NodeList
            foreach (XmlNode regions in regionNodes)
            {
                // get the name attribute for the region
                XmlAttributeCollection attributes = regions.Attributes;
                XmlNode nameNode = attributes.GetNamedItem("name");
                if (nameNode == null) Console.WriteLine("Attribute 'name' not found.");
                else Console.WriteLine(" " + ++count + ". " + nameNode.InnerText);
            }

            bool valid = false;
            Console.Write("\nEnter a region #: ");

            do
            {
                int choice = Convert.ToInt32(Console.ReadLine());
                if (choice < 1 || choice > 15)
                {
                    Console.WriteLine("Choice must be between 1 and 15");
                    valid = false;
                }
                else
                {
                    // obtain names of each source description in the selected region
                    XmlNodeList getSourceDescription = doc.SelectNodes($"/ghg-canada/region[{choice}]/source/@description");

                    // obtain emissions within selected year range in the selected region                   
                    XmlNodeList getEmissions = doc.SelectNodes($"/ghg-canada/region[{choice}]/source/emissions[@year >= {yearsEntered[0]} and @year <= {yearsEntered[1]}]");

                    // used to calculate how many columns are needed to be displayed
                    int totalYears = yearsEntered[1] - yearsEntered[0] + 1;

                    // to store the 8 source description names
                    string[] sourceDescArray = new string[8];

                    // indicate if a new column should be created
                    int createNewColumn = -1;

                    // load array with the 8 source description names
                    for (int i = 0; i < 8; i++)
                    {
                        sourceDescArray[i] += getSourceDescription[i].InnerXml;
                    }

                    Console.WriteLine($"\nEmissions in {regionList[choice - 1].InnerXml} (Megatonnes)");
                    Console.WriteLine("--------------------------------\n");
                    Console.Write("\t\t\t\t\t\tSource\t\t");

                    // display the selected years
                    for (int i = yearsEntered[0]; i < yearsEntered[0] + totalYears; i++)
                    {
                        Console.Write(i + "\t");
                    }

                    Console.WriteLine("\n");
                    Console.Write("\t\t\t\t\t\t");

                    // display the first source description "Agriculture"
                    Console.Write(sourceDescArray[0]);

                    bool newLine = false;
                    // if there is a new line increment this (used to display source descriptions after each line)
                    int sourceDescCounter = 0;

                    // foreach to display emissions
                    foreach (XmlNode emissions in getEmissions)
                    {
                        createNewColumn++;

                        // create a new column for every year
                        if (createNewColumn % totalYears == 0 && createNewColumn != 0)
                        {
                            sourceDescCounter++;
                            Console.Write("\n");
                            newLine = true;
                        }
                        // if its not the long name, add spaces
                        if (newLine && sourceDescCounter == 3) Console.Write("\t");
                        if (newLine && sourceDescCounter != 3) Console.Write("\t\t\t\t\t\t");
                        // display the final 7 source descriptions "Buildings ... Total, with formatting "
                        if (newLine && sourceDescCounter < 6) Console.Write(sourceDescArray[sourceDescCounter]);
                        if (newLine && sourceDescCounter >= 6) Console.Write(sourceDescArray[sourceDescCounter] + "\t");
                        newLine = false;

                        // format all years to 3 decimal places
                        string formattedEmissionOutput = String.Format("{0:#,0.000}", emissions.CreateNavigator().ValueAsDouble);
                        Console.Write("\t" + formattedEmissionOutput);
                    }
                    // add 2 spaces after output is done printing
                    Console.WriteLine("\n");
                    valid = true;
                }
            } while (!valid);
        }

        private static void DisplayDescriptions(XmlDocument doc)
        {
            // array to hold value of year range
            int[] yearsEntered = new int[2];

            SetYears(yearsEntered);

            // get all region names
            XmlNodeList regionList = doc.SelectNodes("/ghg-canada//region/@name");

            // used to calculate how many columns are needed to be displayed
            int totalYears = yearsEntered[1] - yearsEntered[0] + 1;

            // indicate if a new column should be created
            int createNewColumn = -1;

            // get a NodeList of all source elements
            XmlNodeList sourceNodes = doc.GetElementsByTagName("source");
            Console.WriteLine(Environment.NewLine + "Select a source by number as shown below...");
            int count = 0;

            // loop through all description elements in the NodeList
            foreach (XmlNode descriptions in sourceNodes)
            {
                count++;
                if (count == 9) break;
                // get the description attribute for the source
                XmlAttributeCollection attributes = descriptions.Attributes;
                XmlNode nameNode = attributes.GetNamedItem("description");
                if (nameNode == null) Console.WriteLine("Attribute 'description' not found.");
                else Console.WriteLine(" " + count + ". " + nameNode.InnerText);
            }

            bool valid = false;

            do
            {
                Console.Write("\nEnter a source #: ");
                int choice = Convert.ToInt32(Console.ReadLine());
                if (choice < 1 || choice > 8)
                {
                    Console.WriteLine("Choice must be between 1 and 8");
                }
                else
                {
                    string[] descNames = { "Agriculture", "Buildings", "Heavy Industry", "Light Manufacturing, Construction and Forest Resources",
                        "Oil and Gas", "Transport", "Waste", "Total" };


                    Console.WriteLine($"\nEmissions from {descNames[choice - 1]} (Megatonnes)");
                    Console.WriteLine("--------------------------------\n");
                    Console.Write("\t\t\t\tRegion\t\t\t\t\t");

                    // display the selected years
                    for (int i = yearsEntered[0]; i < yearsEntered[0] + totalYears; i++)
                    {
                        Console.Write(i + "\t");
                    }

                    Console.WriteLine("\n");

                    XmlNodeList getDataBySource = doc.SelectNodes($"/ghg-canada/region/source[@description='{descNames[choice - 1]}']/emissions[@year >= {yearsEntered[0]} and @year <= {yearsEntered[1]}]");

                    bool newLine = false;
                    int regionListIncrementer = 0;

                    Console.Write("\t\t\t\t" + regionList[0].InnerText + "\t\t\t\t");
                    foreach (XmlNode dataBySource in getDataBySource)
                    {

                        createNewColumn++;

                        // create a new column for every year
                        if (createNewColumn % totalYears == 0 && createNewColumn != 0)
                        {
                            Console.Write("\n");
                            newLine = true;
                            regionListIncrementer++;
                        }


                        if (newLine)
                        {
                            if (regionListIncrementer >= 0 && regionListIncrementer <= 14)
                            {
                                // ugly manual spacing
                                if (regionListIncrementer == 1) Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText + "\t\t");
                                else if (regionListIncrementer == 2) Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText + "\t\t\t");
                                else if (regionListIncrementer == 3) Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText + "\t\t\t");
                                else if (regionListIncrementer == 4) Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText + "\t");
                                else if (regionListIncrementer == 5) Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText + "\t\t");
                                else if (regionListIncrementer == 7) Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText + "\t\t\t");
                                else if (regionListIncrementer == 8) Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText + "\t\t\t\t");
                                else if (regionListIncrementer == 9) Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText + "\t\t\t\t");
                                else if (regionListIncrementer == 10) Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText + "\t\t");
                                else if (regionListIncrementer == 11) Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText + "\t\t\t\t");
                                else if (regionListIncrementer == 12) Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText + "\t\t\t");
                                else if (regionListIncrementer == 13)
                                {
                                    regionListIncrementer = 14;
                                    Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText + "\t\t\t\t");
                                }
                                else Console.Write("\t\t\t\t" + regionList[regionListIncrementer].InnerText);
                            }
                        }
                        newLine = false;

                        string formattedAllOutput = String.Format("{0:#,0.000}", dataBySource.CreateNavigator().ValueAsDouble);
                        Console.Write("\t" + formattedAllOutput);
                    }
                    // add 2 spaces after output is done printing
                    Console.WriteLine("\n");
                    valid = true;
                }
            } while (!valid);        
        }    
    }
}