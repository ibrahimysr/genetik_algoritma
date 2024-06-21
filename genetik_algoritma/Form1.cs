using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace genetik_algoritma
{
    public partial class Form1 : Form
    {
      
        public Form1()
        {
            InitializeComponent(); 
        }

       
        private void button1_Click(object sender, EventArgs e)
        {
          
            int populationSize = int.Parse(textBox1.Text); 
            double crossoverRate = double.Parse(textBox2.Text); 
            double mutationRate = double.Parse(textBox3.Text); 
            int elitismCount = int.Parse(textBox4.Text);
            int generationCount = int.Parse(textBox5.Text); 

            
            Func<double[], double> fitness = x =>
            {
                double sum = 0; 
                for (int i = 0; i < x.Length - 1; i++)
                {
                    // Rosenbrock fonksiyonunun hesaplanması
                    sum += 100 * Math.Pow(x[i + 1] - x[i], 2) + Math.Pow(x[i] - 1, 2);
                }
                return sum; 
            };

           
            var ga = new GeneticAlgorithm(populationSize, crossoverRate, mutationRate, elitismCount);
          
            var bestSolution = ga.Solve(fitness, generationCount);

            // Sonuçların gösterilmesi
            richTextBox1.Text = string.Join(" - ", bestSolution); 
            textBox7.Text = fitness(bestSolution).ToString();

            PlotConvergence(ga.GetConvergenceHistory()); 
        }

     
        private void PlotConvergence(List<double> convergenceHistory)
        {
          
            chart1.Series.Clear(); 
            chart1.Titles.Clear();

           
            double maxFitnessValue = convergenceHistory.Max(); 
            chart1.ChartAreas[0].AxisY.Minimum = 0; 
            chart1.ChartAreas[0].AxisY.Maximum = maxFitnessValue; 

           
            chart1.ChartAreas[0].AxisY.Interval = maxFitnessValue / 10; 
            chart1.ChartAreas[0].AxisY.IntervalType = DateTimeIntervalType.Number; 

            double maxIteration = convergenceHistory.Count - 1;
            chart1.ChartAreas[0].AxisX.Minimum = 0; 
            chart1.ChartAreas[0].AxisX.Maximum = maxIteration; 

            Series series = new Series(); 
            series.ChartType = SeriesChartType.Line; 
            series.BorderWidth = 2; 
            series.Color = Color.FromArgb(135, 87, 92); 

           
            for (int i = 0; i < convergenceHistory.Count; i++)
            {
                series.Points.AddXY(i, convergenceHistory[i]); 
            }

            chart1.Series.Add(series);

          
            chart1.ChartAreas[0].AxisX.Title = "İterasyon"; 
            chart1.ChartAreas[0].AxisY.Title = "Uygunluk Değeri"; 

           
            chart1.Invalidate(); 
        }
    }
}



public class GeneticAlgorithm
{
    private int populationSize; 
    private double crossoverRate; 
    private double mutationRate; 
    private int elitismCount;
    private List<double> convergenceHistory; 
    
    private static readonly Random Random = new Random();

 
    public GeneticAlgorithm(int populationSize, double crossoverRate, double mutationRate, int elitismCount)
    {
        this.populationSize = populationSize; 
        this.crossoverRate = crossoverRate; 
        this.mutationRate = mutationRate; 
        this.elitismCount = elitismCount; 
        this.convergenceHistory = new List<double>();
    }

    public double[] Solve(Func<double[], double> fitness, int generationCount)
    {
        // Popülasyon oluşturulur
        var population = new List<double[]>(); 
        for (int i = 0; i < populationSize; i++)
        {
            population.Add(RandomArray(10, -5, 10)); 
        }

        // Jenerasyonlar boyunca döngü yapılır
        for (int i = 0; i < generationCount; i++)
        {
            // Uygunluk değerleri hesaplanır
            var fitnessValues = new List<double>();
            foreach (var chromosome in population)
            {
                fitnessValues.Add(fitness(chromosome));
            }
            convergenceHistory.Add(fitnessValues.Min()); 

            // Seçilim işlemi yapılır
            var selectedChromosomes = new List<double[]>(); 
            for (int j = 0; j < populationSize; j++)
            {
                selectedChromosomes.Add(SelectChromosome(population, fitnessValues)); 
            }

            // Çaprazlama işlemi yapılır
            var newPopulation = new List<double[]>(); 
            for (int j = 0; j < populationSize; j++)
            {
                if (Random.NextDouble() < crossoverRate)
                {
                    var parent1 = selectedChromosomes[j];
                    var parent2 = selectedChromosomes[Random.Next(populationSize)]; 
                    newPopulation.Add(Crossover(parent1, parent2)); 
                }
                else
                {
                    newPopulation.Add(selectedChromosomes[j]); 
                }
            }

            // Mutasyon işlemi yapılır
            foreach (var chromosome in newPopulation)
            {
                Mutate(chromosome, mutationRate);
            }

            // Elitizm işlemi yapılır
            var elitistChromosomes = new List<double[]>();
            for (int j = 0; j < elitismCount; j++)
            {

                elitistChromosomes.Add(population[fitnessValues.IndexOf(fitnessValues.Min())]);
            }

            // Yeni popülasyon oluşturulur
            population = new List<double[]>(elitistChromosomes); 
            population.AddRange(newPopulation); 
        }

        // En iyi çözüm döndürülür
        var bestFitness = fitness(population[0]); 
        var bestChromosome = population[0]; 
        foreach (var chromosome in population)
        {
            var chromosomeFitness = fitness(chromosome); 
            if (chromosomeFitness < bestFitness)
            {
                bestFitness = chromosomeFitness; 
                bestChromosome = chromosome; 
            }
        }
        return bestChromosome; 
    }

    // Yakınsama geçmişi getirme metodu
    public List<double> GetConvergenceHistory()
    {
        return convergenceHistory; 
    }

    // Rastgele dizi oluşturma metodu
    private double[] RandomArray(int size, double min, double max)
    {
        var array = new double[size]; 
        for (int i = 0; i < size; i++)
        {
            array[i] = Random.NextDouble() * (max - min) + min; 
        }
        return array; 
    }

    // Birey seçme metodu turuva metodu
    private double[] SelectChromosome(List<double[]> population, List<double> fitnessValues)
    {
        int index1 = Random.Next(population.Count); 
        int index2 = Random.Next(population.Count); 
        while (index1 == index2)
        {
            index2 = Random.Next(population.Count); 
        }
        return fitnessValues[index1] < fitnessValues[index2] ? population[index1] : population[index2]; 
    }

    // Çaprazlama metodu tek noktalı çaprazlama 
    private double[] Crossover(double[] parent1, double[] parent2)
    {
        int crossoverPoint = Random.Next(parent1.Length - 1); 
        var child = new double[parent1.Length]; 
        for (int i = 0; i < crossoverPoint; i++)
        {
            child[i] = parent1[i]; 
        }
        for (int i = crossoverPoint; i < parent1.Length; i++)
        {
            child[i] = parent2[i];
        }
        return child""s; 
    }

    // Mutasyon metodu
    private void Mutate(double[] chromosome, double mutationRate)
    {
        for (int i = 0; i < chromosome.Length; i++)
        {
            if (Random.NextDouble() < mutationRate)
            {
                chromosome[i] += Random.NextDouble() * 0.2 - 0.1; 
                chromosome[i] = Math.Max(chromosome[i], -5); 
                chromosome[i] = Math.Min(chromosome[i], 10);
            }
        }
    }
}