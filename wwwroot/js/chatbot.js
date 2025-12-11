const chatBody = document.querySelector(".chat-body");
const messageInput = document.querySelector(".message-input");
const sendMessageButton = document.querySelector("#send-message");
const fileInput = document.querySelector("#file-input");
const fileUploadWrapper = document.querySelector(".file-upload-wrapper");
const fileCancelButton = document.querySelector("#file-cancel");
const chatbotToggler = document.querySelector("#chatbot-toggler");
const closeChatbot = document.querySelector("#close-chatbot");

// API setup - Load keys from backend API (from appsettings.json)
let API_KEY = null;
let OPENAI_API_KEY = null;
const API_URL_BASE = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
const DALL_E_API_URL = "https://api.openai.com/v1/images/generations";

// Load API keys from backend
async function loadApiKeys() {
    try {
        const response = await fetch('/api/chatbot/config');
        const config = await response.json();
        API_KEY = config.geminiApiKey;
        OPENAI_API_KEY = config.openAIApiKey;
    } catch (error) {
        console.error('Error loading API keys:', error);
    }
}

// Initialize API keys on page load
loadApiKeys();

// Helper function to get API URL with key
function getApiUrl() {
    return `${API_URL_BASE}?key=${API_KEY}`;
}

const userData = {
    message: null,
    file: {
        data: null,
        mime_type: null
    }
};

// Training data từ JSON
let trainingData = null;
let systemPrompt = "";

// Load training data từ JSON
const loadTrainingData = async() => {
    try {
        const response = await fetch('/data/chatbot_training_data.json');
        const data = await response.json();
        trainingData = data;
        systemPrompt = data.system_prompt;
        console.log('Training data loaded successfully:', data);
    } catch (error) {
        console.error('Error loading training data:', error);
        // Fallback system prompt
        systemPrompt = "Bạn là MonMon, chatbot chuyên nghiệp của Mon Amour - dịch vụ hẹn hò cao cấp. Hãy trả lời một cách thân thiện và hữu ích.";
    }
};

// Context về Mon Amour cho chatbot
const chatHistory = [{
    role: "model",
    parts: [{
        text: `Tôi là trợ lý AI của Mon Amour - nền tảng cung cấp dịch vụ lên kế hoạch hẹn hò cá nhân hóa trọn gói. 

Mon Amour chuyên:
- Tổ chức các buổi hẹn lãng mạn, ấn tượng và ý nghĩa
- Dịch vụ hóa cảm xúc - biến những khoảnh khắc tình cảm thành trải nghiệm được thiết kế chỉn chu
- Cung cấp các concept hẹn hò đa dạng và sáng tạo
- Tư vấn và hỗ trợ khách hàng lên kế hoạch hẹn hò hoàn hảo

Tôi có thể giúp bạn:
✨ Tư vấn các ý tưởng hẹn hò lãng mạn
💝 Gợi ý quà tặng phù hợp
🎯 Lên kế hoạch buổi hẹn chi tiết
📍 Tìm địa điểm hẹn hò lý tưởng
💡 Giải đáp thắc mắc về dịch vụ Mon Amour

Hãy cho tôi biết bạn cần hỗ trợ gì nhé!`
    }],
}, ];

const initialInputHeight = messageInput.scrollHeight;

// Create message element with dynamic classes and return it
const createMessageElement = (content, ...classes) => {
    const div = document.createElement("div");
    div.classList.add("message", ...classes);
    div.innerHTML = content;
    return div;
};

// Tìm câu trả lời phù hợp từ training data
const findBestAnswer = (userMessage) => {
    if (!trainingData || !trainingData.training_data) return null;

    const userMessageLower = userMessage.toLowerCase();
    let bestMatch = null;
    let bestScore = 0;

    // Tìm kiếm theo keywords
    for (const item of trainingData.training_data) {
        let score = 0;

        // Kiểm tra keywords
        if (item.keywords) {
            for (const keyword of item.keywords) {
                if (userMessageLower.includes(keyword.toLowerCase())) {
                    score += 2;
                }
            }
        }

        // Kiểm tra câu hỏi tương tự
        const questionWords = item.question.toLowerCase().split(' ');
        const userWords = userMessageLower.split(' ');
        const commonWords = questionWords.filter(word => userWords.includes(word));
        score += commonWords.length * 0.5;

        if (score > bestScore) {
            bestScore = score;
            bestMatch = item;
        }
    }

    return bestScore > 1 ? bestMatch : null;
};

// Kiểm tra xem người dùng có yêu cầu tạo ảnh không
const isImageGenerationRequest = (userMessage) => {
    const imageKeywords = [
        'tạo ảnh', 'tạo hình', 'vẽ ảnh', 'vẽ hình', 'tạo hình ảnh', 'sinh ảnh',
        'generar imagen', 'generate image', 'create image', 'tạo cho tôi ảnh',
        'show me', 'cho tôi xem', 'visualize', 'visual', 'hình dung',
        'không gian hẹn hò', 'concept hẹn hò', 'phong cách hẹn hò'
    ];
    const messageLower = userMessage.toLowerCase();
    return imageKeywords.some(keyword => messageLower.includes(keyword.toLowerCase()));
};

// Generate image using DALL-E 3 API or fallback to Gemini description
const generateImage = async(incomingMessageDiv) => {
    const messageElement = incomingMessageDiv.querySelector(".message-text");

    try {
        // Load API keys if not already loaded
        if (!OPENAI_API_KEY || !API_KEY) {
            await loadApiKeys();
        }

        // Nếu có OpenAI API key, sử dụng DALL-E 3 để tạo ảnh
        if (OPENAI_API_KEY && OPENAI_API_KEY.trim() !== "") {
            // Tạo prompt tối ưu cho DALL-E 3
            const dallePrompt = `Professional romantic dating space setup, ${userData.message}, elegant romantic atmosphere, soft warm lighting, beautiful decorations with flowers and candles, high quality, photorealistic, interior design, cozy intimate setting`;

            const requestOptions = {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${OPENAI_API_KEY}`
                },
                body: JSON.stringify({
                    model: "dall-e-3",
                    prompt: dallePrompt,
                    n: 1,
                    size: "1024x1024",
                    quality: "standard"
                })
            };

            const response = await fetch(DALL_E_API_URL, requestOptions);
            const data = await response.json();

            if (!response.ok) {
                throw new Error(data.error?.message || "Có lỗi xảy ra khi tạo ảnh");
            }

            // Hiển thị ảnh được tạo bởi DALL-E 3
            const imageUrl = data.data[0].url;
            messageElement.innerHTML = `
                <div style="margin-bottom: 10px;">
                    <img src="${imageUrl}" style="max-width: 100%; border-radius: 10px; box-shadow: 0 4px 8px rgba(0,0,0,0.1);" alt="Không gian hẹn hò được tạo" />
                </div>
                <p style="color: #666; font-size: 0.9rem; margin-top: 10px;">
                    🎨 Đây là concept không gian hẹn hò lãng mạn cho bạn! 💝<br>
                    Bạn có muốn tôi tư vấn về các gói dịch vụ Mon Amour để biến concept này thành hiện thực không?
                </p>
            `;
            return;
        }

        // Fallback: Sử dụng Gemini để mô tả concept nếu không có DALL-E API key
        const imagePrompt = `Bạn là chuyên gia thiết kế không gian hẹn hò lãng mạn của Mon Amour. Hãy mô tả chi tiết một concept không gian hẹn hò lãng mạn theo yêu cầu: "${userData.message}". 

Hãy mô tả một cách sống động và chi tiết về:
- Không gian, bố cục, màu sắc
- Ánh sáng và không khí lãng mạn
- Các chi tiết trang trí (hoa, nến, khăn trải bàn, v.v.)
- Cảm giác và trải nghiệm tổng thể

Hãy viết một đoạn mô tả dài khoảng 200-300 từ, sử dụng ngôn ngữ thơ mộng, gợi cảm để người đọc có thể hình dung rõ ràng về không gian hẹn hò này.`;

        const requestOptions = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                contents: [{
                    role: "user",
                    parts: [{ text: imagePrompt }]
                }]
            })
        };

        // Wait for API key to be loaded if not already loaded
        if (!API_KEY) {
            await loadApiKeys();
        }

        const response = await fetch(getApiUrl(), requestOptions);
        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.error?.message || "Có lỗi xảy ra khi tạo concept");
        }

        // Extract and display bot's response text
        const apiResponseText = data.candidates[0].content.parts[0].text.replace(/\*\*(.*?)\*\*/g, "$1").trim();

        messageElement.innerHTML = `
            <div style="padding: 15px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 10px; color: white; line-height: 1.8;">
                <h4 style="margin: 0 0 15px 0; font-size: 1.1rem; display: flex; align-items: center; gap: 8px;">
                    🎨 Concept Không Gian Hẹn Hò Mon Amour
                </h4>
                <p style="margin: 0; font-size: 0.95rem; text-align: justify; white-space: pre-line;">
                    ${apiResponseText}
                </p>
                <div style="margin-top: 15px; padding-top: 15px; border-top: 1px solid rgba(255,255,255,0.3); font-size: 0.9rem;">
                    💝 <strong>Lưu ý:</strong> Để tạo hình ảnh thực tế, vui lòng cấu hình OpenAI DALL-E API key trong file chatbot.js
                </div>
            </div>
        `;

        chatHistory.push({
            role: "model",
            parts: [{ text: apiResponseText }]
        });
    } catch (error) {
        console.error("Image Generation Error:", error);
        messageElement.innerHTML = `
            <div style="color: #ff0000; font-size: 0.9rem;">
                <strong>Xin lỗi!</strong><br>
                ${error.message}<br><br>
                <em>Vui lòng thử lại sau hoặc liên hệ với chúng tôi qua:</em><br>
                📞 Hotline: 0868019255<br>
                📧 Email: booking.monamour@gmail.com
            </div>
        `;
    } finally {
        userData.file = {};
        incomingMessageDiv.classList.remove("thinking");
        chatBody.scrollTo({ behavior: "smooth", top: chatBody.scrollHeight });
    }
};

// Generate bot response using API
const generateBotResponse = async(incomingMessageDiv) => {
    const messageElement = incomingMessageDiv.querySelector(".message-text");

    // Tìm câu trả lời từ training data trước
    const trainingAnswer = findBestAnswer(userData.message);

    let contextualMessage;
    if (trainingAnswer) {
        // Sử dụng câu trả lời từ training data
        contextualMessage = `${systemPrompt}\n\nDựa trên thông tin training data, hãy trả lời câu hỏi: "${userData.message}"\n\nThông tin tham khảo: ${trainingAnswer.answer}`;
    } else {
        // Sử dụng context chung
        contextualMessage = `${systemPrompt}\n\nHãy trả lời câu hỏi sau một cách thân thiện và hữu ích: ${userData.message}`;
    }

    chatHistory.push({
        role: "user",
        parts: [{ text: contextualMessage }, ...(userData.file.data ? [{ inline_data: userData.file }] : [])],
    });

    // API request options
    const requestOptions = {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            contents: chatHistory
        })
    }

    try {
        // Wait for API key to be loaded if not already loaded
        if (!API_KEY) {
            await loadApiKeys();
        }

        // Kiểm tra xem API key đã được cấu hình chưa
        if (!API_KEY || API_KEY === "null") {
            throw new Error("Vui lòng cấu hình API key của Google Gemini trong appsettings.json");
        }

        // Fetch bot response from API
        const response = await fetch(getApiUrl(), requestOptions);
        const data = await response.json();
        if (!response.ok) throw new Error(data.error?.message || "Có lỗi xảy ra khi kết nối API");

        // Extract and display bot's response text
        const apiResponseText = data.candidates[0].content.parts[0].text.replace(/\*\*(.*?)\*\*/g, "$1").trim();
        messageElement.innerText = apiResponseText;
        chatHistory.push({
            role: "model",
            parts: [{ text: apiResponseText }]
        });
    } catch (error) {
        console.error("Chatbot Error:", error);
        messageElement.innerHTML = `
            <div style="color: #ff0000; font-size: 0.9rem;">
                <strong>Xin lỗi!</strong><br>
                ${error.message}<br><br>
                <em>Vui lòng thử lại sau hoặc liên hệ với chúng tôi qua:</em><br>
                📞 Hotline: 0868019255<br>
                📧 Email: booking.monamour@gmail.com
            </div>
        `;
    } finally {
        userData.file = {};
        incomingMessageDiv.classList.remove("thinking");
        chatBody.scrollTo({ behavior: "smooth", top: chatBody.scrollHeight });
    }
};

// Handle outgoing user message
const handleOutgoingMessage = (e) => {
        e.preventDefault();
        userData.message = messageInput.value.trim();
        messageInput.value = "";
        fileUploadWrapper.classList.remove("file-uploaded");
        messageInput.dispatchEvent(new Event("input"));

        // Create and display user message
        const messageContent = `<div class="message-text"></div>
                            ${userData.file.data ? `<img src="data:${userData.file.mime_type};base64,${userData.file.data}" class="attachment" />` : ""}`;

    const outgoingMessageDiv = createMessageElement(messageContent, "user-message");
    outgoingMessageDiv.querySelector(".message-text").innerText = userData.message;
    chatBody.appendChild(outgoingMessageDiv);
    chatBody.scrollTop = chatBody.scrollHeight;

    // Simulate bot response with thinking indicator after a delay
    setTimeout(() => {
        const messageContent = `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
                    <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
                </svg>
                <div class="message-text">
                    <div class="thinking-indicator">
                        <div class="dot"></div>
                        <div class="dot"></div>
                        <div class="dot"></div>
                    </div>
                </div>`;

        const incomingMessageDiv = createMessageElement(messageContent, "bot-message", "thinking");
        chatBody.appendChild(incomingMessageDiv);
        chatBody.scrollTo({ behavior: "smooth", top: chatBody.scrollHeight });
        
        // Kiểm tra xem có phải yêu cầu tạo ảnh không
        if (isImageGenerationRequest(userData.message)) {
            generateImage(incomingMessageDiv);
        } else {
            generateBotResponse(incomingMessageDiv);
        }
    }, 600);
};

// Handle Enter key press for sending messages
messageInput.addEventListener("keydown", (e) => {
    const userMessage = e.target.value.trim();
    if (e.key === "Enter" && userMessage && !e.shiftKey && window.innerWidth > 768) {
        handleOutgoingMessage(e);
    }
});

messageInput.addEventListener("input", (e) => {
    messageInput.style.height = `${initialInputHeight}px`;
    messageInput.style.height = `${messageInput.scrollHeight}px`;
    document.querySelector(".chat-form").style.borderRadius = messageInput.scrollHeight > initialInputHeight ? "15px" : "32px";
});

// Handle file input change event
fileInput.addEventListener("change", async (e) => {
    const file = e.target.files[0];
    if (!file) return;
    
    const validImageTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    if (!validImageTypes.includes(file.type)) {
        if (typeof Swal !== 'undefined') {
            await Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: 'Chỉ chấp nhận file ảnh (JPEG, PNG, GIF, WEBP)',
                confirmButtonText: 'OK'
            });
        } else {
            alert('Chỉ chấp nhận file ảnh (JPEG, PNG, GIF, WEBP)');
        }
        resetFileInput();
        return;
    }
    
    const reader = new FileReader();
    reader.onload = (e) => {
        fileUploadWrapper.querySelector("img").src = e.target.result;
        fileUploadWrapper.classList.add("file-uploaded");
        const base64String = e.target.result.split(",")[1];
        userData.file = {
            data: base64String,
            mime_type: file.type
        };
    };
    reader.readAsDataURL(file);
});

fileCancelButton.addEventListener("click", (e) => {
    userData.file = {};
    fileUploadWrapper.classList.remove("file-uploaded");
});

function resetFileInput() {
    fileInput.value = "";
    fileUploadWrapper.classList.remove("file-uploaded");
    fileUploadWrapper.querySelector("img").src = "#";
    userData.file = { data: null, mime_type: null };
}

// Initialize emoji picker if EmojiMart is available
if (typeof EmojiMart !== 'undefined') {
    const picker = new EmojiMart.Picker({
        theme: "light",
        showSkinTones: "none",
        previewPosition: "none",
        onEmojiSelect: (emoji) => {
            const { selectionStart: start, selectionEnd: end } = messageInput;
            messageInput.setRangeText(emoji.native, start, end, "end");
            messageInput.focus();
        },
        onClickOutside: (e) => {
            if (e.target.id === "emoji-picker") {
                document.body.classList.toggle("show-emoji-picker");
            } else {
                document.body.classList.remove("show-emoji-picker");
            }
        },
    });

    document.querySelector(".chat-form").appendChild(picker);
}

// Event listeners
sendMessageButton.addEventListener("click", (e) => handleOutgoingMessage(e));
document.querySelector("#file-upload").addEventListener("click", (e) => fileInput.click());
chatbotToggler.addEventListener("click", () => document.body.classList.toggle("show-chatbot"));
closeChatbot.addEventListener("click", () => document.body.classList.remove("show-chatbot"));

// Initialize chatbot with welcome message
document.addEventListener('DOMContentLoaded', async function() {
    // Load training data trước
    await loadTrainingData();
    
    // Add welcome message to chat body
    const welcomeMessageContent = `<svg class="bot-avatar" xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 1024 1024">
                <path d="M738.3 287.6H285.7c-59 0-106.8 47.8-106.8 106.8v303.1c0 59 47.8 106.8 106.8 106.8h81.5v111.1c0 .7.8 1.1 1.4.7l166.9-110.6 41.8-.8h117.4l43.6-.4c59 0 106.8-47.8 106.8-106.8V394.5c0-59-47.8-106.9-106.8-106.9zM351.7 448.2c0-29.5 23.9-53.5 53.5-53.5s53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5-53.5-23.9-53.5-53.5zm157.9 267.1c-67.8 0-123.8-47.5-132.3-109h264.6c-8.6 61.5-64.5 109-132.3 109zm110-213.7c-29.5 0-53.5-23.9-53.5-53.5s23.9-53.5 53.5-53.5 53.5 23.9 53.5 53.5-23.9 53.5-53.5 53.5zM867.2 644.5V453.1h26.5c19.4 0 35.1 15.7 35.1 35.1v121.1c0 19.4-15.7 35.1-35.1 35.1h-26.5zM95.2 609.4V488.2c0-19.4 15.7-35.1 35.1-35.1h26.5v191.3h-26.5c-19.4 0-35.1-15.7-35.1-35.1zM561.5 149.6c0 23.4-15.6 43.3-36.9 49.7v44.9h-30v-44.9c-21.4-6.5-36.9-26.3-36.9-49.7 0-28.6 23.3-51.9 51.9-51.9s51.9 23.3 51.9 51.9z"></path>
            </svg>
            <div class="message-text">Xin chào! 👋<br><br>Tôi là trợ lý AI của <strong>Mon Amour</strong> - nền tảng dịch vụ hẹn hò cá nhân hóa.<br><br>Tôi có thể giúp bạn:<br>💝 Tư vấn ý tưởng hẹn hò lãng mạn<br>🎁 Gợi ý quà tặng ý nghĩa<br>📍 Tìm địa điểm hẹn hò lý tưởng<br>🎨 Thiết kế concept không gian hẹn hò đặc biệt (chỉ cần nói "tạo concept hẹn hò" hoặc "mô tả không gian lãng mạn")<br>💡 Giải đáp về dịch vụ Mon Amour<br><br>Hãy cho tôi biết bạn cần hỗ trợ gì nhé!</div>`;

    const welcomeMessageDiv = createMessageElement(welcomeMessageContent, "bot-message");
    if (chatBody) {
        chatBody.appendChild(welcomeMessageDiv);
    }
});