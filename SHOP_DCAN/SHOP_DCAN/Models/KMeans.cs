using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SHOP_DCAN.Models
{
    public class KMeans
    {
        public List<Cluster> Clusters { get; private set; }

        public KMeans(int k, List<SanPham> products)
        {
            Clusters = new List<Cluster>();
            InitializeClusters(k, products);
            AssignPointsToClusters(products);
            UpdateClusters();
        }

        private void InitializeClusters(int k, List<SanPham> products)
        {
            Random random = new Random();
            for (int i = 0; i < k; i++)
            {
                int index = random.Next(products.Count);
                Clusters.Add(new Cluster(products[index]));
            }
        }

        private void AssignPointsToClusters(List<SanPham> products)
        {
            foreach (var product in products)
            {
                Cluster closestCluster = null;
                double minDistance = double.MaxValue;

                foreach (var cluster in Clusters)
                {
                    double distance = ComputeEuclideanDistance(product, cluster.Centroid);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestCluster = cluster;
                    }
                }

                closestCluster?.AddProduct(product);
            }
        }

        private void UpdateClusters()
        {
            foreach (var cluster in Clusters)
            {
                cluster.UpdateCentroid();
            }
        }

        private double ComputeEuclideanDistance(SanPham product1, SanPham product2)
        {
            double sum = Math.Pow((int)product1.MaLoai - (int)product2.MaLoai, 2) +
                         Math.Pow((int)product1.MaTH - (int)product2.MaTH, 2) +
                         Math.Pow((double)product1.DonGia - (double)product2.DonGia, 2);
            return Math.Sqrt(sum);
        }
    }

    public class Cluster
    {
        public SanPham Centroid { get; private set; }
        public List<SanPham> Products { get; private set; }

        public Cluster(SanPham initialCentroid)
        {
            Centroid = initialCentroid;
            Products = new List<SanPham>();
        }

        public void AddProduct(SanPham product)
        {
            Products.Add(product);
        }

        public void UpdateCentroid()
        {
            if (Products.Count == 0)
            {
                return;
            }

            double sumMaLoai = Products.Sum(p => (int)p.MaLoai);
            double sumMaTH = Products.Sum(p => (int)p.MaTH);
            decimal sumDonGia = (decimal)Products.Sum(p => p.DonGia);

            Centroid = new SanPham
            {
                MaLoai = (int)(sumMaLoai / Products.Count),
                MaTH = (int)(sumMaTH / Products.Count),
                DonGia = sumDonGia / Products.Count
            };
        }
    }


}