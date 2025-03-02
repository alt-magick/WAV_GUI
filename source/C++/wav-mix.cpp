#include <iostream>
#include <fstream>
#include <vector>
#include <cmath>
#include <cstdint>
#include <limits>
#include <algorithm>
#include <iterator>
#include <string>

// WAV file header structure
struct WAVHeader {
    char riff[4];
    uint32_t fileSize;
    char wave[4];
    char fmt[4];
    uint32_t fmtSize;
    uint16_t audioFormat;
    uint16_t numChannels;
    uint32_t sampleRate;
    uint32_t byteRate;
    uint16_t blockAlign;
    uint16_t bitsPerSample;
    char data[4];
    uint32_t dataSize;
};

// Function to read WAV file
bool readWAV(const std::string& filename, WAVHeader& header, std::vector<int16_t>& samples) {
    std::ifstream file(filename, std::ios::binary);
    if (!file) return false;
    file.read(reinterpret_cast<char*>(&header), sizeof(WAVHeader));

    size_t numSamples = header.dataSize / (header.bitsPerSample / 8) / header.numChannels;
    samples.resize(numSamples * header.numChannels);  // Stereo: left and right channel per sample

    file.read(reinterpret_cast<char*>(samples.data()), header.dataSize);
    file.close();
    return true;
}

// Function to write WAV file
bool writeWAV(const std::string& filename, WAVHeader header, const std::vector<int16_t>& samples) {
    header.dataSize = samples.size() * sizeof(int16_t);
    header.fileSize = header.dataSize + sizeof(WAVHeader) - 8;

    std::ofstream file(filename, std::ios::binary);
    if (!file) return false;

    file.write(reinterpret_cast<const char*>(&header), sizeof(WAVHeader));
    file.write(reinterpret_cast<const char*>(samples.data()), header.dataSize);
    file.close();
    return true;
}

// Crossfade blending with selected curve
enum CrossfadeCurve {
    EXPONENTIAL,
    LOGARITHMIC,
    SIGMOID,
    AVERAGE,
    ADDITIVE,
    SUBTRACTIVE
};

// Apply selected blending curve
double applyBlendCurve(double i, size_t numSamples, CrossfadeCurve curveType) {
    double blendFactor = 0.0;

    switch (curveType) {
    case EXPONENTIAL:
        blendFactor = std::exp(static_cast<double>(i) / numSamples) - 1.0; // Exponential curve
        break;
    case LOGARITHMIC:
        blendFactor = std::log(static_cast<double>(i + 1)) / std::log(static_cast<double>(numSamples + 1)); // Logarithmic curve
        break;
    case SIGMOID:
        blendFactor = 1.0 / (1.0 + std::exp(-10.0 * (static_cast<double>(i) / numSamples - 0.5))); // Sigmoid curve
        break;
    }

    return std::min(1.0, blendFactor);  // Ensure blend factor doesn't exceed 1.0
}

// Time-aligned layering with selected crossfade curve
std::vector<int16_t> timeAlignedLayering(const std::vector<int16_t>& signal1, const std::vector<int16_t>& signal2, int offset, uint16_t numChannels, CrossfadeCurve curveType, double crossfadePercentage) {
    size_t numSamples = std::min(signal1.size(), signal2.size()) / numChannels;
    std::vector<int16_t> aligned(numSamples * numChannels);

    for (size_t i = 0; i < numSamples; ++i) {
        for (uint16_t ch = 0; ch < numChannels; ++ch) {
            size_t index = i * numChannels + ch;
            int shiftedIndex = index + offset * numChannels;

            if (shiftedIndex >= 0 && shiftedIndex < signal2.size()) {
                // Get the blend factor based on the selected curve
                double blendFactor = applyBlendCurve(static_cast<double>(i), numSamples, curveType);
                blendFactor *= (crossfadePercentage / 100.0);  // Apply user-defined mixing percentage

                // Apply the blend based on the calculated blendFactor
                aligned[index] = static_cast<int16_t>((signal1[index] * (1.0 - blendFactor)) + (signal2[shiftedIndex] * blendFactor));
            }
        }
    }
    return aligned;
}

// Align and blend two audio signals (Stereo)
std::vector<int16_t> Avrg(const std::vector<int16_t>& samples1, const std::vector<int16_t>& samples2, uint16_t numChannels, double crossfadePercentage) {
    size_t numSamples = std::min(samples1.size(), samples2.size()) / numChannels;
    std::vector<int16_t> aligned(numSamples * numChannels);

    for (size_t i = 0; i < numSamples; ++i) {
        for (uint16_t ch = 0; ch < numChannels; ++ch) {
            size_t index = i * numChannels + ch;
            aligned[index] = static_cast<int16_t>((samples1[index] * (1.0 - crossfadePercentage)) + (samples2[index] * crossfadePercentage));
        }
    }
    return aligned;
}

// Align and blend two audio signals (Stereo)
std::vector<int16_t> Additive(const std::vector<int16_t>& samples1, const std::vector<int16_t>& samples2, uint16_t numChannels, double crossfadePercentage) {
    size_t numSamples = std::min(samples1.size(), samples2.size()) / numChannels;
    std::vector<int16_t> aligned(numSamples * numChannels);

    for (size_t i = 0; i < numSamples; ++i) {
        for (uint16_t ch = 0; ch < numChannels; ++ch) {
            size_t index = i * numChannels + ch;
            aligned[index] = std::max(-32768, std::min(32767, samples1[index] + samples2[index]));
        }
    }
    return aligned;
}


// Align and blend two audio signals (Stereo)
std::vector<int16_t> Subtractive(const std::vector<int16_t>& samples1, const std::vector<int16_t>& samples2, uint16_t numChannels, double crossfadePercentage) {
    size_t numSamples = std::min(samples1.size(), samples2.size()) / numChannels;
    std::vector<int16_t> aligned(numSamples * numChannels);

    for (size_t i = 0; i < numSamples; ++i) {
        for (uint16_t ch = 0; ch < numChannels; ++ch) {
            size_t index = i * numChannels + ch;
            aligned[index] = std::max(-32768, std::min(32767, samples1[index] - samples2[index]));
        }
    }
    return aligned;
}

int main(int argc, char* argv[]) {
    if (argc < 6) {
        std::cerr << std::endl << "Usage: " << argv[0] << " <input1.wav> <input2.wav> <output.wav> <method> <mix_percentage>" << std::endl << "Methods: (1) Exponential, (2) Logarithmic, (3) Sigmoid, (4) Average, (5) Additive, (6) Subtractive" << std::endl << std::endl;
        return 1;
    }
    std::string file1 = argv[1];
    std::string file2 = argv[2];
    std::string outputFile = argv[3];
    int curveSelection = std::atoi(argv[4]);
    double mixPercentage = std::stod(argv[5]);

    WAVHeader header1, header2;
    std::vector<int16_t> samples1, samples2;

    if (!readWAV(file1, header1, samples1) || !readWAV(file2, header2, samples2)) {
        std::cerr << "Error: Could not read input files." << std::endl;
        return 1;
    }

    // Check if both files have the same format (sample rate, bit depth, and number of channels)
    if (header1.sampleRate != header2.sampleRate || header1.bitsPerSample != header2.bitsPerSample ||
        header1.numChannels != header2.numChannels) {
        std::cerr << "Error: WAV files must have the same format." << std::endl;
        return 1;
    }

    // Map user's choice to CrossfadeCurve enum
    CrossfadeCurve curveType = EXPONENTIAL;  // Default to exponential
    switch (curveSelection) {
    case 1:
        curveType = EXPONENTIAL;
        break;
    case 2:
        curveType = LOGARITHMIC;
        break;
    case 3:
        curveType = SIGMOID;
        break;
    case 4:
        curveType = AVERAGE;
        break;
    case 5:
        curveType = ADDITIVE;
        break;
    case 6:
        curveType = SUBTRACTIVE;
        break;
    default:
        std::cout << "Invalid choice, defaulting to Exponential." << std::endl;
    }

    // Perform blending based on the selected curve and mix percentage
    std::vector<int16_t> alignedSamples;
    if (curveType == AVERAGE) {
        alignedSamples = Avrg(samples1, samples2, header1.numChannels, mixPercentage / 100.0);
    }
    if (curveType == ADDITIVE) {
        alignedSamples = Additive(samples1, samples2, header1.numChannels, mixPercentage / 100.0);
    }
    if (curveType == SUBTRACTIVE) {
        alignedSamples = Subtractive(samples1, samples2, header1.numChannels, mixPercentage / 100.0);
    }
    else {
        int bestOffset = 500;  // Assuming the best offset is known (could be calculated using cross-correlation)
        alignedSamples = timeAlignedLayering(samples1, samples2, bestOffset, header1.numChannels, curveType, mixPercentage);
    }

    // Write the resulting audio to a new file
    if (!writeWAV(outputFile, header1, alignedSamples)) {
        std::cerr << "Error: Could not write output file." << std::endl;
        return 1;
    }

    std::cout << "Time-aligned layered WAV saved as: " << outputFile << std::endl;
    return 0;
}
