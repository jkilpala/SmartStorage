using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace SmartStoragePi.Helpers
{
    class I2CLightHelper
    {
        private I2cDevice i2CDevice;
        private Timer periodicTimer;
        private byte deviceAddress;
        //List<bool[]> lightList;
        List<List<bool[]>> lightPathArray;
        public int row { get; set; }
        public int column { get; set; }
        int maxRows = 3;
        int maxColumns = 2;

        public async void Init(byte address, int loopSpeed)
        {
            row = 1;
            column = 1;
            deviceAddress = address;
            var settings = new I2cConnectionSettings(deviceAddress);
            settings.BusSpeed = I2cBusSpeed.FastMode;
            var controller = await I2cController.GetDefaultAsync();
            i2CDevice = controller.GetDevice(settings);

            System.Diagnostics.Debug.WriteLine("I2C init completed");
            periodicTimer = new Timer(this.TimerCallback, null, 0, loopSpeed);

            //Populate list for easy access
            lightPathArray = new List<List<bool[]>>();
            lightPathArray.Add(new List<bool[]>());
            lightPathArray[0].Add(LightControlConstants.ALL_OFF);

            lightPathArray.Add(new List<bool[]>());
            lightPathArray[1].Add(LightControlConstants.ALL_OFF);
            lightPathArray[1].Add(LightControlConstants.PATH_TO_1_1);
            lightPathArray[1].Add(LightControlConstants.PATH_TO_1_2);

            lightPathArray.Add(new List<bool[]>());
            lightPathArray[2].Add(LightControlConstants.ALL_OFF);
            lightPathArray[2].Add(LightControlConstants.PATH_TO_2_1);
            lightPathArray[2].Add(LightControlConstants.PATH_TO_2_2);

            lightPathArray.Add(new List<bool[]>());
            lightPathArray[3].Add(LightControlConstants.ALL_OFF);
            lightPathArray[3].Add(LightControlConstants.PATH_TO_3_1);
            lightPathArray[3].Add(LightControlConstants.PATH_TO_3_2);

            lightPathArray.Add(new List<bool[]>());
            lightPathArray[4].Add(LightControlConstants.ALL_OFF);

            System.Diagnostics.Debug.WriteLine("Just to stop for a moment");


            //WriteLightSettingToLights(lightPathArray[1][1]);
        }
        public void TestScrollRow()
        {
            if (row + 1 > maxRows)
            {
                row = 1;
            }
            else
            {
                row++;
            }
        }
        public void TestScrollColumn()
        {
            if (column + 1 > maxColumns)
            {
                column = 1;
            }
            else
            {
                column++;
            }
        }
        private void TimerCallback(object state)
        {
            try
            {
                // WriteToI2C();
                WriteLightSettingToLights(lightPathArray[row][column]);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("I2CHelper::TimerCallback:" + ex.Message);
            }
        }

        private void ReadFromI2C()
        {
            byte[] writeBuffer = new byte[4];
            i2CDevice.Read(writeBuffer);
            float value = BitConverter.ToSingle(writeBuffer, 0);
            System.Diagnostics.Debug.WriteLine("value:" + value);

        }
        float tf = 0.5f;
        private void WriteToI2C()
        {
            byte[] writeBuffer = new byte[4];
            writeBuffer = BitConverter.GetBytes(tf);
            i2CDevice.Write(writeBuffer);
            tf += 0.01f;
            //float value = BitConverter.ToSingle(writeBuffer, 0);
            //System.Diagnostics.Debug.WriteLine("value:" + value);
        }
        private void WriteLightSettingToLights(bool[] direction)
        {
            byte[] writeBuffer = new byte[9];

            writeBuffer = ByteUtils.PackBoolsInByteArray(direction);//ToByteArray(direction);
            i2CDevice.Write(writeBuffer);

        }

        public void Release()
        {
            i2CDevice.Dispose();
        }
        static byte[] ToByteArray(bool[] input)
        {
            if (input.Length % 8 != 0)
            {
                throw new ArgumentException("input");
            }
            byte[] ret = new byte[input.Length / 8];
            for (int i = 0; i < input.Length; i += 8)
            {
                int value = 0;
                for (int j = 0; j < 8; j++)
                {
                    if (input[i + j])
                    {
                        value += 1 << (7 - j);
                    }
                }
                ret[i / 8] = (byte)value;
            }
            return ret;
        }
    }
}
