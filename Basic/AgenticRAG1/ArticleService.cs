using AgenticRAG1.Models;

namespace AgenticRAG1;

public class ArticleService
{
    private readonly VectorStoreService _vectorStore;
    private int _nextId = 1;

    public ArticleService(VectorStoreService vectorStore)
    {
        _vectorStore = vectorStore;
    }

    public async Task InitializeDatabaseAsync()
    {
        await _vectorStore.InitializeAsync();
        await SeedArticlesAsync();
    }

    private async Task SeedArticlesAsync()
    {
        var articles = new List<Article>
        {
            new Article
            {
                Id = _nextId++,
                Title = "The Future of Artificial Intelligence in Healthcare",
                Content = "Artificial Intelligence is revolutionizing healthcare by enabling faster diagnosis, personalized treatment plans, and predictive analytics. Machine learning algorithms can analyze medical images with accuracy comparable to expert radiologists. AI-powered systems are helping doctors identify diseases earlier, leading to better patient outcomes. Natural language processing is being used to extract insights from medical records and research papers. As AI continues to evolve, we can expect more innovative applications in drug discovery, robotic surgery, and patient monitoring.",
                Category = "Technology",
                PublishDate = new DateTime(2024, 1, 15)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Climate Change and Global Carbon Emissions",
                Content = "Global carbon emissions have reached unprecedented levels, contributing to accelerated climate change. Scientists warn that without immediate action, we face severe consequences including rising sea levels, extreme weather events, and ecosystem collapse. Countries around the world are implementing carbon reduction strategies, investing in renewable energy, and promoting sustainable practices. The transition to clean energy requires collaboration between governments, businesses, and individuals. Recent climate summits have highlighted the urgency of limiting global temperature rise to 1.5 degrees Celsius.",
                Category = "Environment",
                PublishDate = new DateTime(2024, 2, 10)
            },
            new Article
            {
                Id = _nextId++,
                Title = "The Rise of Remote Work and Digital Nomads",
                Content = "The COVID-19 pandemic accelerated the shift to remote work, fundamentally changing how we think about employment. Companies are now offering flexible work arrangements, and many employees prefer working from home. Digital nomads are embracing location-independent lifestyles, working while traveling the world. This trend has implications for urban planning, real estate markets, and company culture. Tools like video conferencing, project management software, and cloud computing have made remote collaboration seamless. The future of work is likely to be hybrid, combining the best of both in-office and remote models.",
                Category = "Business",
                PublishDate = new DateTime(2024, 1, 20)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Quantum Computing Breakthrough at Major Tech Lab",
                Content = "Researchers have achieved a major breakthrough in quantum computing, demonstrating quantum advantage in solving complex optimization problems. This advancement brings us closer to practical quantum computers that can tackle challenges beyond the reach of classical computers. Applications include drug discovery, cryptography, financial modeling, and climate simulation. The team used a new error-correction technique that significantly improved qubit stability. Major tech companies are investing billions in quantum research, racing to build the first commercially viable quantum computer.",
                Category = "Technology",
                PublishDate = new DateTime(2024, 3, 5)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Mediterranean Diet Linked to Longevity",
                Content = "A comprehensive study spanning two decades has confirmed that the Mediterranean diet is associated with increased longevity and reduced risk of chronic diseases. The diet emphasizes fruits, vegetables, whole grains, olive oil, and fish while limiting red meat and processed foods. Researchers found that adherence to this dietary pattern reduces the risk of heart disease, diabetes, and certain cancers. The anti-inflammatory properties of olive oil and omega-3 fatty acids play a crucial role. Nutritionists recommend incorporating Mediterranean diet principles for optimal health.",
                Category = "Health",
                PublishDate = new DateTime(2024, 2, 28)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Electric Vehicles Surpass 10 Million Global Sales",
                Content = "Electric vehicle sales have reached a historic milestone, exceeding 10 million units globally. This represents a 55% increase from the previous year, driven by improved battery technology, expanded charging infrastructure, and government incentives. Major automakers are phasing out internal combustion engines and investing heavily in EV production. Battery costs have decreased by 80% over the past decade, making EVs more affordable. Analysts predict that EVs will reach price parity with gasoline vehicles within the next few years, accelerating adoption.",
                Category = "Technology",
                PublishDate = new DateTime(2024, 3, 12)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Ancient City Discovered Using Satellite Technology",
                Content = "Archaeologists have discovered a previously unknown ancient city using advanced satellite imaging and LiDAR technology. The site, located in a dense jungle, reveals sophisticated urban planning with temples, residential areas, and irrigation systems. This discovery challenges existing theories about ancient civilizations in the region. The technology allowed researchers to map the site without extensive excavation, preserving the archaeological integrity. Preliminary dating suggests the city flourished over 1,500 years ago and was home to thousands of inhabitants.",
                Category = "Science",
                PublishDate = new DateTime(2024, 1, 25)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Global Stock Markets Rally on Economic Data",
                Content = "Major stock markets worldwide experienced significant gains following the release of positive economic indicators. Strong employment numbers, moderate inflation, and robust consumer spending boosted investor confidence. Technology and financial sectors led the rally, with major indices reaching new highs. Central banks signaled a pause in interest rate hikes, further supporting market optimism. Analysts remain cautiously optimistic about sustained growth, noting that geopolitical tensions and supply chain issues could still pose risks.",
                Category = "Business",
                PublishDate = new DateTime(2024, 2, 14)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Breakthrough in Alzheimer's Disease Treatment",
                Content = "Clinical trials of a new Alzheimer's treatment have shown promising results in slowing cognitive decline. The drug targets amyloid plaques in the brain, a hallmark of the disease. Patients receiving the treatment showed a 27% reduction in cognitive decline compared to the placebo group. This represents the most significant advancement in Alzheimer's therapy in decades. While not a cure, the treatment offers hope for millions of patients and their families. Researchers are now investigating combination therapies to enhance effectiveness.",
                Category = "Health",
                PublishDate = new DateTime(2024, 3, 8)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Space Tourism Becomes Reality for Civilians",
                Content = "Commercial space tourism has officially launched, with several private companies offering suborbital flights to paying customers. The first civilian missions have successfully completed, providing passengers with breathtaking views of Earth and brief periods of weightlessness. Tickets currently cost hundreds of thousands of dollars, but companies project prices will decrease as technology improves and competition increases. This marks a new era in space exploration, making space accessible beyond professional astronauts. Future plans include orbital hotels and lunar tourism.",
                Category = "Science",
                PublishDate = new DateTime(2024, 2, 22)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Cybersecurity Threats Evolve with AI Technology",
                Content = "Cybersecurity experts warn that artificial intelligence is being weaponized by hackers to create more sophisticated attacks. AI-powered malware can adapt to security measures, making detection difficult. Phishing attacks using deepfake technology have become increasingly convincing. Organizations are responding by implementing AI-driven security systems that can identify and respond to threats in real-time. The cybersecurity industry is experiencing unprecedented demand for skilled professionals. Experts recommend regular security audits, employee training, and multi-factor authentication as essential defenses.",
                Category = "Technology",
                PublishDate = new DateTime(2024, 1, 30)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Renewable Energy Now Cheaper Than Fossil Fuels",
                Content = "For the first time in history, renewable energy sources are consistently cheaper than fossil fuels in most markets worldwide. Solar and wind energy costs have plummeted due to technological improvements and economies of scale. This economic shift is accelerating the transition to clean energy, with countries rapidly decommissioning coal plants. Energy storage solutions are addressing intermittency challenges, making renewables viable for baseload power. Investment in renewable energy infrastructure has surpassed fossil fuel investments for three consecutive years.",
                Category = "Environment",
                PublishDate = new DateTime(2024, 3, 1)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Gene Editing Successfully Treats Sickle Cell Disease",
                Content = "Doctors have successfully treated sickle cell disease using CRISPR gene editing technology, marking a milestone in genetic medicine. The treatment involves editing patients' own blood stem cells to produce healthy hemoglobin. Follow-up studies show patients remain disease-free with no adverse effects. This breakthrough offers hope for treating thousands of genetic disorders. The therapy is currently expensive and complex, but researchers are working to make it more accessible. Ethical discussions continue regarding the appropriate use of gene editing technology.",
                Category = "Health",
                PublishDate = new DateTime(2024, 2, 18)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Blockchain Technology Transforms Supply Chain Management",
                Content = "Companies are adopting blockchain technology to revolutionize supply chain transparency and efficiency. The technology enables real-time tracking of products from manufacturer to consumer, reducing fraud and ensuring authenticity. Major retailers and logistics companies are implementing blockchain solutions to streamline operations and reduce costs. Smart contracts automate payments and compliance verification, eliminating intermediaries. Despite initial skepticism, blockchain has proven valuable beyond cryptocurrency applications. Industry analysts predict widespread adoption within the next five years.",
                Category = "Business",
                PublishDate = new DateTime(2024, 1, 12)
            },
            new Article
            {
                Id = _nextId++,
                Title = "New Species Discovered in Deep Ocean Expedition",
                Content = "Marine biologists have discovered dozens of new species during a deep-sea exploration mission. The expedition utilized advanced submersibles and ROVs to explore previously unreachable ocean depths. Discovered species include bioluminescent fish, unique crustaceans, and microorganisms that thrive in extreme conditions. These findings expand our understanding of marine biodiversity and evolution. Scientists believe millions of species remain undiscovered in the world's oceans. The research has implications for biotechnology, medicine, and understanding climate change impacts on marine ecosystems.",
                Category = "Science",
                PublishDate = new DateTime(2024, 3, 15)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Mental Health Apps Gain Popularity and Effectiveness",
                Content = "Mental health applications have seen explosive growth, providing accessible support for millions of users worldwide. Studies show that well-designed apps can effectively complement traditional therapy for anxiety, depression, and stress management. Features include cognitive behavioral therapy exercises, meditation guides, mood tracking, and AI-powered chatbots. The convenience and affordability of these apps are reducing barriers to mental health care. However, experts emphasize that apps should supplement, not replace, professional treatment for serious conditions. Privacy concerns remain regarding sensitive health data.",
                Category = "Health",
                PublishDate = new DateTime(2024, 2, 5)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Autonomous Vehicles Begin Commercial Operations",
                Content = "Self-driving vehicles have started commercial operations in several major cities, marking a significant milestone in transportation technology. The autonomous taxi services operate in geofenced areas with safety drivers initially on board. Companies report thousands of successful trips with minimal incidents. The technology uses advanced sensors, AI, and real-time mapping to navigate complex urban environments. Regulatory frameworks are evolving to accommodate autonomous vehicles while ensuring public safety. Industry leaders predict autonomous vehicles will be widespread within the next decade, transforming urban mobility.",
                Category = "Technology",
                PublishDate = new DateTime(2024, 3, 20)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Global Coral Reef Restoration Shows Promising Results",
                Content = "International coral reef restoration projects are showing encouraging results, with survival rates exceeding expectations. Scientists are using innovative techniques including coral gardening, 3D-printed reef structures, and selective breeding of heat-resistant coral species. These efforts are critical as climate change threatens these vital ecosystems that support 25% of marine life. Community involvement has been key to project success, with local populations trained in restoration techniques. While challenges remain, these projects demonstrate that active intervention can help preserve coral reefs for future generations.",
                Category = "Environment",
                PublishDate = new DateTime(2024, 1, 8)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Cryptocurrency Market Stabilizes After Volatile Period",
                Content = "After years of extreme volatility, the cryptocurrency market is showing signs of maturation and stabilization. Institutional investors are entering the market with sophisticated strategies and risk management. Regulatory clarity in major economies has reduced uncertainty, encouraging mainstream adoption. Bitcoin and Ethereum remain dominant, but numerous altcoins are finding practical applications in finance, gaming, and digital identity. Central banks are exploring digital currencies, potentially legitimizing the broader crypto ecosystem. Experts debate whether crypto will complement or disrupt traditional financial systems.",
                Category = "Business",
                PublishDate = new DateTime(2024, 2, 25)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Personalized Medicine Revolutionizes Cancer Treatment",
                Content = "Advances in genomic sequencing are enabling personalized cancer treatments tailored to individual patients' genetic profiles. Oncologists can now identify specific mutations driving cancer growth and select targeted therapies with higher success rates. Immunotherapy approaches are training patients' immune systems to recognize and attack cancer cells. Survival rates for several cancer types have improved dramatically with these precision medicine approaches. The cost of genomic sequencing has decreased significantly, making personalized treatment more accessible. Research continues into combination therapies and early detection methods.",
                Category = "Health",
                PublishDate = new DateTime(2024, 3, 10)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Vertical Farming Addresses Urban Food Security",
                Content = "Vertical farming technology is emerging as a solution to urban food security challenges and agricultural sustainability. These indoor farms use LED lighting, hydroponics, and controlled environments to grow crops year-round with minimal water and no pesticides. Urban vertical farms can produce significantly more food per square foot than traditional agriculture. The technology reduces transportation costs and emissions while providing fresh produce to city populations. Major food retailers are partnering with vertical farming companies. While energy costs remain a challenge, improvements in efficiency are making the model increasingly viable.",
                Category = "Environment",
                PublishDate = new DateTime(2024, 1, 18)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Brain-Computer Interfaces Enable Paralyzed Patients to Communicate",
                Content = "Revolutionary brain-computer interface technology has enabled paralyzed patients to communicate by translating their thoughts into text and speech. The system uses implanted electrodes to decode neural signals associated with attempted speech movements. Patients can now communicate at rates approaching natural conversation, dramatically improving quality of life. The technology represents decades of neuroscience research and engineering innovation. Researchers are working to make the devices less invasive and more affordable. Future applications may include controlling prosthetic limbs and treating neurological disorders.",
                Category = "Technology",
                PublishDate = new DateTime(2024, 2, 8)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Ancient Manuscripts Decoded Using AI",
                Content = "Artificial intelligence has successfully decoded ancient manuscripts that had puzzled scholars for centuries. Machine learning algorithms trained on known ancient languages identified patterns in the mysterious texts. The decoded manuscripts reveal insights into historical civilizations, including their culture, trade networks, and scientific knowledge. This breakthrough demonstrates AI's potential in humanities research. Historians are now applying similar techniques to other undeciphered texts and damaged documents. The discovery has generated renewed interest in archaeology and ancient history among younger generations.",
                Category = "Science",
                PublishDate = new DateTime(2024, 3, 3)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Sustainable Fashion Movement Gains Corporate Support",
                Content = "Major fashion brands are embracing sustainability, responding to consumer demand for environmentally responsible products. Companies are adopting circular economy principles, using recycled materials, and ensuring fair labor practices. Innovative fabrics made from mushrooms, algae, and recycled plastic are replacing traditional materials. The fast fashion model is being challenged by rental services and second-hand marketplaces. Transparency in supply chains is increasing through blockchain technology. While greenwashing remains a concern, genuine progress is being made toward sustainable fashion industry transformation.",
                Category = "Business",
                PublishDate = new DateTime(2024, 1, 22)
            },
            new Article
            {
                Id = _nextId++,
                Title = "Exercise Found to Enhance Cognitive Function and Memory",
                Content = "Comprehensive research confirms that regular physical exercise significantly enhances cognitive function, memory, and mental health. Exercise increases blood flow to the brain, promotes neuroplasticity, and stimulates the release of beneficial hormones. Studies show that both aerobic exercise and strength training provide cognitive benefits across all age groups. Even moderate activity like walking for 30 minutes daily can improve brain health. The findings emphasize exercise as a powerful intervention for preventing cognitive decline and supporting mental wellness. Experts recommend combining physical activity with social engagement for optimal benefits.",
                Category = "Health",
                PublishDate = new DateTime(2024, 2, 12)
            }
        };

        Console.WriteLine($"Inserting {articles.Count} articles and generating embeddings...");
        foreach (var article in articles)
        {
            await _vectorStore.AddArticleAsync(article);
            Console.Write(".");
        }
        Console.WriteLine($"\nSuccessfully inserted {articles.Count} articles into the vector store.");
    }

    public async Task<List<Article>> SearchArticlesAsync(string keyword)
    {
        var records = await _vectorStore.SearchArticlesAsync(keyword);
        return records.Select(r => new Article
        {
            Id = r.OriginalId,
            Title = r.Title,
            Content = r.Content,
            Category = r.Category,
            PublishDate = r.PublishDate
        }).ToList();
    }

    public async Task<Article?> GetArticleByIdAsync(int id)
    {
        var record = await _vectorStore.GetArticleByIdAsync(id);
        if (record == null) return null;
        
        return new Article
        {
            Id = record.OriginalId,
            Title = record.Title,
            Content = record.Content,
            Category = record.Category,
            PublishDate = record.PublishDate
        };
    }

    public async Task<List<Article>> GetArticlesByCategoryAsync(string category)
    {
        var allRecords = await _vectorStore.GetAllArticlesAsync();
        return allRecords
            .Where(r => r.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .Select(r => new Article
            {
                Id = r.OriginalId,
                Title = r.Title,
                Content = r.Content,
                Category = r.Category,
                PublishDate = r.PublishDate
            })
            .ToList();
    }

    public async Task<List<Article>> GetAllArticlesAsync()
    {
        var records = await _vectorStore.GetAllArticlesAsync();
        return records.Select(r => new Article
        {
            Id = r.OriginalId,
            Title = r.Title,
            Content = r.Content,
            Category = r.Category,
            PublishDate = r.PublishDate
        }).ToList();
    }
}
