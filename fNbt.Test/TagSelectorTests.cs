using NUnit.Framework;

namespace fNbt.Test {
    [TestFixture]
    public sealed class TagSelectorTests {
        [Test]
        public void SkippingTagsOnFileLoad() {
            var loadedFile = new NbtFile();
            loadedFile.LoadFromFile(TestFiles.Big,
                                    NbtCompression.None,
                                    tag => tag.Name != "nested compound test");
            var rootTag = (NbtCompound)loadedFile.RootTag;
            Assert.IsFalse(rootTag.Contains("nested compound test"));
            Assert.IsTrue(rootTag.Contains("listTest (long)"));

            loadedFile.LoadFromFile(TestFiles.Big,
                                    NbtCompression.None,
                                    tag => tag.TagType != NbtTagType.Float || tag.Parent.Name != "Level");
            rootTag = (NbtCompound)loadedFile.RootTag;
            Assert.IsFalse(rootTag.Contains("floatTest"));
            Assert.AreEqual(0.75f, loadedFile.RootTag["nested compound test"]["ham"]["value"].FloatValue);

            loadedFile.LoadFromFile(TestFiles.Big,
                                    NbtCompression.None,
                                    tag => tag.Name != "listTest (long)");
            rootTag = (NbtCompound)loadedFile.RootTag;
            Assert.IsFalse(rootTag.Contains("listTest (long)"));
            Assert.IsTrue(rootTag.Contains("byteTest"));

            loadedFile.LoadFromFile(TestFiles.Big,
                                    NbtCompression.None,
                                    tag => false);
            rootTag = (NbtCompound)loadedFile.RootTag;
            Assert.AreEqual(0, rootTag.Count);
        }


        [Test]
        public void SkippingLists() {
            {
                var file = new NbtFile(TestFiles.MakeListTest());
                byte[] savedFile = file.SaveToBuffer(NbtCompression.None);
                file.LoadFromBuffer(savedFile, 0, savedFile.Length, NbtCompression.None,
                                    tag => tag.TagType != NbtTagType.List);
                var rootTag = (NbtCompound)file.RootTag;
                Assert.AreEqual(0, rootTag.Count);
            }
            {
                // Check list-compound interaction
                NbtCompound comp = new NbtCompound("root") {
                    new NbtCompound("compOfLists") {
                        new NbtList("listOfComps") {
                            new NbtCompound {
                                new NbtList("emptyList", NbtTagType.Compound)
                            }
                        }
                    }
                };
                var file = new NbtFile(comp);
                byte[] savedFile = file.SaveToBuffer(NbtCompression.None);
                file.LoadFromBuffer(savedFile, 0, savedFile.Length, NbtCompression.None,
                                    tag => tag.TagType != NbtTagType.List);
                var rootTag = (NbtCompound)file.RootTag;
                Assert.AreEqual(1, rootTag.Count);
            }
        }


        [Test]
        public void SkippingValuesInCompoundTest() {
            NbtCompound root = TestFiles.MakeValueTest();
            NbtCompound nestedComp = TestFiles.MakeValueTest();
            nestedComp.Name = "NestedComp";
            root.Add(nestedComp);

            var file = new NbtFile(root);
            byte[] savedFile = file.SaveToBuffer(NbtCompression.None);
            file.LoadFromBuffer(savedFile, 0, savedFile.Length, NbtCompression.None, tag => false);
            var rootTag = (NbtCompound)file.RootTag;
            Assert.AreEqual(0, rootTag.Count);
        }
    }
}
