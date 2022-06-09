using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class MapHandler
    {
        public static int width = 235, height = 155;
        public static EntityWrapper[,] BigArray = new EntityWrapper[width, height];
        Loader loader = new Loader();
        private void FillArrayEntities()
        {
            loader.Convert();

            foreach (var item in Loader.powerEntities)
            {
                PlaceEntity(item);
            }
        }

        private void PlaceEntity(EntityWrapper entity)
        {
            int X = (int) ((entity.powerEntity.X - 45.2325) / ((45.277031 - 45.2325) / width));
            int Y = (int) ((entity.powerEntity.Y - 19.793909) / ((19.894459 - 19.793909) / height));
            if(BigArray[X, Y] == null)
            {
                BigArray[X, Y] = new EntityWrapper();
                BigArray[X, Y].powerEntity = entity.powerEntity;
                BigArray[X, Y].entityType = entity.entityType;
                return;
            }
            EntityWrapper temp = BigArray[X, Y];
            while(temp.nextEntity != null)
            {
                temp = temp.nextEntity;
            }
            temp.nextEntity = new EntityWrapper();
            temp.nextEntity.powerEntity = entity.powerEntity;
            temp.nextEntity.entityType = entity.entityType;
        }

        public void CalculateEntities()
        {
            FillArrayEntities();
        }

        public static Tuple<int, int> GetTuple(long Id)
        {
            EntityWrapper entity;
            for(int i = 0; i < width; i++)
                for(int j = 0; j < height; j++)
                    if(BigArray[i,j] != null)
                    {
                        entity = BigArray[i,j];
                        if(entity.powerEntity.Id == Id)
                            return Tuple.Create(i, j);
                        while (entity.nextEntity != null)
                        {
                            entity = entity.nextEntity;
                            if (entity.powerEntity.Id == Id)
                                return Tuple.Create(i, j);
                        }
                    }    
            return null;
        }
    }
}
